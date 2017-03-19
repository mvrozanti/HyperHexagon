using ConvnetSharp;
using DeepQLearning.DRLAgent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

//∮ 
namespace HyperHexagon {
    class Program {
        static QAgent qAgent;
        static string qAgentBrainPath = "C:\\Users\\Nexor\\Desktop\\hyperhexagon.brain";
        static void Main(string[] args) {// b r o k e n
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            if (File.Exists(qAgentBrainPath)) {
                using (FileStream fstream = new FileStream(qAgentBrainPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    qAgent = new BinaryFormatter().Deserialize(fstream) as QAgent;
                    qAgent.Reinitialize();
                }
                Console.WriteLine("QAgent loaded");
            } else {
                var num_inputs = 6; // 9 eyes, each sees 3 numbers (wall, green, red thing proximity)
                var num_actions = 3; // 5 possible angles agent can turn
                var temporal_window = 1; // amount of temporal memory. 0 = agent lives in-the-moment :)
                var network_size = num_inputs * temporal_window + num_actions * temporal_window + num_inputs;


                // config brain
                var layer_defs = new List<LayerDefinition>();

                // the value function network computes a value of taking any of the possible actions
                // given an input state. Here we specify one explicitly the hard way
                // but user could also equivalently instead use opt.hidden_layer_sizes = [20,20]
                // to just insert simple relu hidden layers.
                layer_defs.Add(new LayerDefinition { type = "input", out_sx = 1, out_sy = 1, out_depth = network_size });
                layer_defs.Add(new LayerDefinition { type = "fc", num_neurons = 96, activation = "relu" });
                layer_defs.Add(new LayerDefinition { type = "fc", num_neurons = 96, activation = "relu" });
                layer_defs.Add(new LayerDefinition { type = "fc", num_neurons = 96, activation = "relu" });
                layer_defs.Add(new LayerDefinition { type = "regression", num_neurons = num_actions });

                // options for the Temporal Difference learner that trains the above net
                // by backpropping the temporal difference learning rule.
                //var opt = new Options { method="sgd", learning_rate=0.01, l2_decay=0.001, momentum=0.9, batch_size=10, l1_decay=0.001 };
                Options opt = new Options { method = "adadelta", l2_decay = 0.001, batch_size = 10 };

                TrainingOptions tdtrainer_options = new TrainingOptions();
                tdtrainer_options.temporal_window = temporal_window;
                tdtrainer_options.experience_size = 30000;
                tdtrainer_options.start_learn_threshold = 1000;
                tdtrainer_options.gamma = 0.7;
                tdtrainer_options.learning_steps_total = 200000;
                tdtrainer_options.learning_steps_burnin = 3000;
                tdtrainer_options.epsilon_min = 0.05;
                tdtrainer_options.epsilon_test_time = 0.00;
                tdtrainer_options.layer_defs = layer_defs;
                tdtrainer_options.options = opt;

                DeepQLearn brain = new DeepQLearn(num_inputs, num_actions, tdtrainer_options);
                qAgent = new QAgent(brain);
            }
            qAgent.startlearn();
            new Thread(() => {
                while (true) {
                    if (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond % 31/*arbitrary*/ == 0) {
                        using (FileStream fstream = new FileStream(qAgentBrainPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
                            new BinaryFormatter().Serialize(fstream, qAgent);
                        }
                    }
                    qAgent.tick();
                }
            }).Start();
        }
    }
}
