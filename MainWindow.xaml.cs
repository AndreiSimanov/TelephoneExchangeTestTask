using System.Collections.ObjectModel;
using System.Threading.Channels;
using System.Windows;
using TelephoneExchange.Models;

namespace TelephoneExchange
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Channel<Call> channel = Channel.CreateUnbounded<Call>();
        readonly ObservableCollection<Agent> agents = new();
        readonly ObservableCollection<Call> incomingCalls = new();
        readonly ObservableCollection<Call> processedCalls = new();

        public MainWindow()
        {
            InitializeComponent();
            callsListView.ItemsSource = incomingCalls;
            processedCallsListView.ItemsSource = processedCalls;
            agentslistView.ItemsSource = agents;
            StartCalling();
        }

        void AddAgentClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(agentNameTextBox.Text))
                return;

            string agentName = agentNameTextBox.Text.Trim();
            if (agents.Any(item => string.Equals(item.Name, agentName)))
            {
               MessageBox.Show($"Agent {agentName} already exists, please choose another name.",
                                      "Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                return;
            }
            AddAgent(agentName);
        }

        void RemoveAgentClick(object sender, RoutedEventArgs e)
        {
            if (agentslistView.SelectedItem != null)
            { 
                var agent = agentslistView.SelectedItem as Agent;
                if (agent != null)
                {
                    agents.Remove(agent);
                    agent.Dispose();
                }
            }
        }

        async void AddCallClick(object sender, RoutedEventArgs e) => await AddCall();

        void StartCalling()
        {
            Task.Factory.StartNew(async () =>
            {
                var rnd = new Random();
                while (true)
                {
                    await Task.Delay(rnd.Next(1000, 2000));
                    await AddCall();
                }
            },
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        void AddAgent(string agentName)
        {
            var agent = new Agent(agentName, new CancellationTokenSource());
            agents.Add(agent);
            Task.Factory.StartNew(async () =>
            {
                var token = agent.Cts.Token;
                while (!token.IsCancellationRequested)
                {
                    var call = await channel.Reader.ReadAsync(token);
                    if (call == null)
                        break;

                    await agent.ProcessCall(call);

                    incomingCalls.Remove(call);
                    processedCalls.Add(call);
                }
            },
           CancellationToken.None,
           TaskCreationOptions.None,
           TaskScheduler.FromCurrentSynchronizationContext());
        }

        ValueTask AddCall()
        {
            var call = new Call(Guid.NewGuid());
            incomingCalls.Add(call);
            return channel.Writer.WriteAsync(call);
        }
    }
}