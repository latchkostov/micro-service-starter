using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MicroServiceStarter
{
    class Program
    {
        private static readonly string RepoTempDirName = Path.Combine(Directory.GetCurrentDirectory(), "temp");
        private static UsernamePasswordCredentials _gitCredentials;

        static void Main(string[] args)
        {
            Console.Write("Enter Github username: ");
            var username = Console.ReadLine();
            Console.Write(Environment.NewLine + "Enter Github password: ");
            var password = GetConsolePassword();
            Console.WriteLine();
            _gitCredentials = new UsernamePasswordCredentials { Password = password, Username = username };
            var microServiceDefinitions = GetMicroServiceDefinitions();
            ProcessRepos(microServiceDefinitions);
            Console.WriteLine(Environment.NewLine + "You may press any key to close this window...");
            Console.ReadKey();
        }

        private static string GetConsolePassword()
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (cki.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        Console.Write("\b\0\b");
                        sb.Length--;
                    }

                    continue;
                }

                Console.Write('*');
                sb.Append(cki.KeyChar);
            }

            return sb.ToString();
        }

        private static MicroServiceDefinitions GetMicroServiceDefinitions()
        {
            var fileText = File.ReadAllText("microServiceDefs.json");
            return JsonConvert.DeserializeObject<MicroServiceDefinitions>(fileText);
        }

        private static void ProcessRepos(MicroServiceDefinitions microServiceDefinitions)
        {
            if (Directory.Exists(RepoTempDirName))
                DeleteDirectory(RepoTempDirName);

            Directory.CreateDirectory(RepoTempDirName);

            foreach (var microService in microServiceDefinitions.MicroServices)
            {
                CloneRepo(microService);
                ModifyMicroServiceConfig(microService, microServiceDefinitions.GlobalConfigOverrides);
                StartMicroServices(microService);
            }
        }

        private static void CloneRepo(MicroService microService)
        {
            SetClonedLocation(microService);

            var co = new CloneOptions();
            co.BranchName = microService.GitRepoBranch;
            co.CredentialsProvider = (_url, _user, _cred) => _gitCredentials;
            try
            {
                Repository.Clone(microService.GitRepo, microService.ClonePath, co);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void SetClonedLocation(MicroService microService)
        {
            var lastDash = microService.GitRepo.LastIndexOf('/');
            var folderName = microService.GitRepo.Substring(lastDash+1).Replace(".git", "");
            microService.RepoFolder = folderName;
            var path = Path.Combine(RepoTempDirName, folderName);
            microService.ClonePath = path;
        }

        private static void ModifyMicroServiceConfig(MicroService microService, List<ConfigOverride> globalConfigOverrides)
        {
            var configFileName = $"appsettings.{microService.AspNetCoreEnvironment}.json";
            var projectPath = Path.Combine(microService.ClonePath, microService.ProjectPath);
            var configFilePath = Path.Combine(Path.GetDirectoryName(projectPath), configFileName);
            WriteHeader($"Modifying {microService.RepoFolder} configuration ({configFileName})");

            var configFileText = File.ReadAllText(configFilePath);
            var configJObject = JObject.Parse(configFileText);

            // Process Micro Service level overrides
            ProcessConfigOverrides(configJObject, microService.ConfigOverrides);

            // Process Global Overrides
            ProcessConfigOverrides(configJObject, globalConfigOverrides.Where(x => !microService.ConfigOverrides.Contains(x)));

            var modifiedText = JsonConvert.SerializeObject(configJObject);
            File.WriteAllText(configFilePath, modifiedText);
        }

        private static JObject ProcessConfigOverrides(JObject inputJObject, IEnumerable<ConfigOverride> configOverrides)
        {
            foreach (var configOverride in configOverrides)
            {
                var token = inputJObject.SelectToken(configOverride.PropertySelector);

                if (token is JValue jValue)
                {
                    switch (configOverride.Operation.ToUpper())
                    {
                        case "MODIFY":
                            jValue.Value = configOverride.PropertyValue;
                            break;
                        case "DELETE":
                            jValue.Parent.Remove();
                            break;
                        default:
                            Console.WriteLine("Unknown config override operation: " + configOverride.Operation);
                            break;
                    }
                }
                else if (token is JObject jObject && configOverride.Operation.ToUpper() == "ADD")
                {
                    try
                    {
                        jObject.Add(new JProperty(configOverride.PropertyName, configOverride.PropertyValue));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    Console.WriteLine(token == null
                        ? $"Could not find property selected using {configOverride.PropertySelector}"
                        : $"Operations on JToken of type {token.GetType().Name} not allowed");
                }
            }

            return inputJObject;
        }

        private static void StartMicroServices(MicroService microService)
        {
            WriteHeader($"Starting {microService.RepoFolder} micro service");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project {Path.Combine(microService.ClonePath, microService.ProjectPath)}",
                    CreateNoWindow = false,
                    UseShellExecute = true
                }
            };

            Console.WriteLine($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
            process.Start();
        }

        private static void DeleteDirectory(string targetDir)
        {
            File.SetAttributes(targetDir, FileAttributes.Normal);

            var files = Directory.GetFiles(targetDir);
            var dirs = Directory.GetDirectories(targetDir);

            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

        private static void WriteHeader(string text)
        {
            var border = new string('=', text.Length);
            Console.WriteLine(border);
            Console.WriteLine(text);
            Console.WriteLine(border);
        }
    }
}
