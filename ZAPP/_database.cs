﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Json;
using System.Collections;
using System.Data;
using Mono.Data.Sqlite;
using Android.App;
using Android.Content;
using Android.Content.Res;
using ZAPP;

namespace ZAPP
{
   class _database
    {
        // Context definieren
        private Context context;
        private string taskUrl = "http://192.168.0.143/Cockpit-ZAPP/cockpit-master/api/collections/get/task?token=9d9a3b472d501a972c788077b12fb5/";
        private string userUrl = "http://192.168.0.143/Cockpit-ZAPP/cockpit-master/api/collections/get/user?token=9d9a3b472d501a972c788077b12fb5/";
        private string activityUrl = "http://192.168.0.143/Cockpit-ZAPP/cockpit-master/api/collections/get/activity?token=9d9a3b472d501a972c788077b12fb5/";
        private string clientUrl = "http://192.168.0.143/Cockpit-ZAPP/cockpit-master/api/collections/get/client?token=9d9a3b472d501a972c788077b12fb5/";
        private string educomDB;
        private string ZAPPDB;
      

        // Constructor
        public _database(Context context)
        {
            this.context = context;
            this.createAllDatabases();
        }

        // Database maken
        public string createDatabase(string url, string createTableData, string databaseName, Action<string, string> downloadData) // 
        {
            Resources res = this.context.Resources;
            string app_name = res.GetString(Resource.String.app_name);
            string app_version = res.GetString(Resource.String.app_version);

            Console.WriteLine(createTableData);

            string dbname = $"_db_{app_name}_{app_version}_{databaseName}.sqlite";
            Console.WriteLine(dbname);

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string databasePath = Path.Combine(documentsPath, dbname);

                var connectionString = String.Format("Data Source={0};Version=3;", databasePath);
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        // Table data
                        cmd.CommandText = createTableData;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
                downloadData(url, databasePath);
            
            return databasePath;
        }

        public void createAllDatabases()
        {
            Resources res = this.context.Resources;

            this.educomDB = createDatabase(taskUrl, res.GetString(Resource.String.createTableTask), "educomDB", downloadTaskData);
            Config.log("before zappdb");
            this.ZAPPDB = createDatabase(activityUrl, res.GetString(Resource.String.createTableActivity), "ZAPPDB", downloadActivityData);
            Config.log("before zappdb1");
            this.ZAPPDB = createDatabase(userUrl, res.GetString(Resource.String.createTableUser), "ZAPPDB", downloadUserData);
            Config.log("ENDEND");

        }

        public void downloadActivityData(string url, string databasePath)
        {
            var webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            try
            {
                byte[] myDataBuffer = webClient.DownloadData(url);
                string download = Encoding.ASCII.GetString(myDataBuffer);
                JsonValue value = JsonValue.Parse(download);
                var entries = value["entries"];

                foreach (JsonObject item in entries)
                {

                    Config.log($"{item["task_id"]} = task_id, {item["isCompleted"]} = isCompleted,  {item["activityName"]} = activityName");
                    this.activityToDatabase(item["task_id"], item["isCompleted"], item["activityName"], databasePath);

                }

            }
            catch (WebException)
            {

            }
        }

        public void downloadUserData(string url, string databasePath)
        {
            var webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            try
            {
                byte[] myDataBuffer = webClient.DownloadData(url);
                string download = Encoding.ASCII.GetString(myDataBuffer);
                JsonValue value = JsonValue.Parse(download);
                var entries = value["entries"];

                foreach (JsonObject item in entries)
                {

                    Console.WriteLine($"{item["username"]} = username, {item["password"]} = password");
                    this.userToDatabase(item["username"], item["password"], databasePath);

                }

            }
            catch (WebException)
            {

            }
        }
        public void downloadTaskData(string url, string databasePath)
        {
            var webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;
            try
            {
                byte[] myDataBuffer = webClient.DownloadData(url);
                string download = Encoding.ASCII.GetString(myDataBuffer);
                JsonValue value = JsonValue.Parse(download);
                var entries = value["entries"];
                foreach (JsonObject item in entries)
                {

                    Config.log($"{item["client_id"]} = client_id, {item["user_id"]} = user_id, {item["startTask"]} = startTask, {item["stopTask"]} = stopTask, {item["taskDate"]} = taskDate, {item["taskName"]} = taskName, {item["isCompleted"]} = isCompleted");
                    this.taskToDatabase(item["client_id"], item["user_id"], item["startTask"], item["stopTask"], item["taskDate"], item["taskName"], item["isCompleted"], databasePath);
          
                }

            }
            catch (WebException)
            {

            }
        }

        public void activityToDatabase(string task_id, string isCompleted, string activityName, string dbPath)
        {
            var connectionString = String.Format("Data Source ={0}; Version = 3;", dbPath);
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    // Table data
                    cmd.CommandText = "INSERT INTO activity (task_id, isCompleted, activityName) VALUES (@task_id, @isCompleted, @activityName)";
                    cmd.Parameters.Add(new SqliteParameter("@task_id", task_id));
                    cmd.Parameters.Add(new SqliteParameter("@isCompleted", isCompleted));
                    cmd.Parameters.Add(new SqliteParameter("@activityName", activityName));
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                    Config.log("ACTIVITY INSERTED INTO DB");
                }
                conn.Close();
            }
            this.getAllActivities(dbPath);
        }

        public void userToDatabase(string username, string password, string dbPath)
        {
            var connectionString = String.Format("Data Source ={0}; Version = 3;", dbPath);
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    // Table data
                    cmd.CommandText = "INSERT INTO user (username,password) VALUES (@username, @password)";
                    cmd.Parameters.Add(new SqliteParameter("@username", username));
                    cmd.Parameters.Add(new SqliteParameter("@password", password));
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("USER INSERTED TO DATABASE");
                }
                conn.Close();
            }
            this.getAllUsers(dbPath);
        }

        public void taskToDatabase(string client_id, string user_id, string startTask, string stopTask, string taskDate, string taskName, string isCompleted, string dbPath)
        {
            var connectionString = String.Format("Data Source ={0}; Version = 3;", dbPath);
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    // Table data
                    cmd.CommandText = "INSERT INTO task (client_id, user_id, startTask, stopTask, taskDate, taskName, isCompleted) VALUES (@client_id, @user_id, @startTask, @stopTask, @taskDate, @taskName, @isCompleted)";
                    cmd.Parameters.Add(new SqliteParameter("@client_id", client_id));
                    cmd.Parameters.Add(new SqliteParameter("@user_id", user_id));
                    cmd.Parameters.Add(new SqliteParameter("@startTask", startTask));
                    cmd.Parameters.Add(new SqliteParameter("@stopTask", stopTask));
                    cmd.Parameters.Add(new SqliteParameter("@taskDate", taskDate));
                    cmd.Parameters.Add(new SqliteParameter("@taskName", taskName));
                    cmd.Parameters.Add(new SqliteParameter("@isCompleted", isCompleted));
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("DATA INSERTED TO DATABASE");
                }
                conn.Close();
            }
        }

        public ArrayList getAllActivities(string dbPath)
        {
            ArrayList activityRecords = new ArrayList();
            var connectionString = String.Format("Data Source={0};Version=3;", dbPath);
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM activity";
                    cmd.CommandType = CommandType.Text;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Config.log("ALL ACTIVITIES ARE BEING READ");
                            activityRecords.Add(new activityRecord(reader));
                        }
                    }
                }
                conn.Close();
            }
            Config.log("DATA RETURNED TO ACTIVITYRECORDS");
            return activityRecords;
        }

        public ArrayList getAllUsers(string dbPath)
        {
            ArrayList userRecords = new ArrayList();
            var connectionString = String.Format("Data Source={0};Version=3;", dbPath);
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM user";
                    cmd.CommandType = CommandType.Text;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Config.log("ALL USERS ARE BEING READ");
                            userRecords.Add(new userRecord(reader));
                        }
                    }
                }
                conn.Close();
            }
            Config.log("DATA RETURNED TO USERRECORDS");
            return userRecords;
        }

        public ArrayList getAllTasks(string dbPath)
        {
            ArrayList taskRecords = new ArrayList();
            var connectionString = String.Format("Data Source={0};Version=3;", dbPath);
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM task";
                    cmd.CommandType = CommandType.Text;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Config.log("ALL TASKS ARE BEING READ");
                            taskRecords.Add(new taskRecord(reader));
                        }
                    }
                }
                conn.Close();
            }
            Console.WriteLine("TASKS RETURNED TO TASKRECORDS");
            return taskRecords;
        }
    }
}