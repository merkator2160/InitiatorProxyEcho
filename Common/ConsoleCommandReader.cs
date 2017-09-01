using System;
using System.Collections.Generic;
using Common.Enums;
using Common.Models;
using Common.Models.Event;

namespace Common
{
    public class ConsoleCommandReader
    {
        private readonly Dictionary<String, Action> _commandDictionary;


        public ConsoleCommandReader(ServerType type)
        {
            _commandDictionary = new Dictionary<String, Action>()
            {
                {"start", () =>
                {
                    ApplicationState = ApplicationState.Working;
                    StartCommandEntered.Invoke(this, new StartCommandEventArgs($"{type}... {ApplicationState}"));
                }},
                {"stop", () =>
                {
                    ApplicationState = ApplicationState.Suspended;
                    StopCommandEntered.Invoke(this, new StopCommandEventArgs($"{type}... {ApplicationState}"));
                }},
                {"exit", () =>
                {
                    ApplicationState = ApplicationState.Stopped;
                }},
            };

            ApplicationState = ApplicationState.Suspended;
        }


        public delegate void StartCommandEventHandler(object sender, StartCommandEventArgs e);
        public event StartCommandEventHandler StartCommandEntered = (sender, args) => { };

        public delegate void StopCommandEventHandler(object sender, StopCommandEventArgs e);
        public event StopCommandEventHandler StopCommandEntered = (sender, args) => { };


        // PROPERTIES /////////////////////////////////////////////////////////////////////////////
        public ApplicationState ApplicationState { get; set; }


        // FUNCTIONS //////////////////////////////////////////////////////////////////////////////
        public void Run()
        {
            while (ApplicationState != ApplicationState.Stopped)
            {
                var commandText = Console.ReadLine();
                if (_commandDictionary.ContainsKey(commandText))
                {
                    _commandDictionary[commandText].Invoke();
                }
            }
        }
    }
}