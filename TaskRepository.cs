using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Threading.Tasks;

namespace Cybersecurity_Awareness_Bot
    {
        public class TaskRepository
        {
            private const string ConnectionStringName = "CyberBotDatabase";
            private readonly string _connectionString;

            public TaskRepository()
            {
                ConnectionStringSettings settings =
                    ConfigurationManager.ConnectionStrings[ConnectionStringName];

                if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                {
                    throw new ConfigurationErrorsException(
                        "The CyberBotDatabase connection string is missing from App.config.");
                }

                _connectionString = settings.ConnectionString;
            }

            public async Task EnsureDatabaseReadyAsync()
            {
                const string sql = @"
                CREATE TABLE IF NOT EXISTS cyber_tasks (
                    id INT NOT NULL AUTO_INCREMENT,
                    title VARCHAR(150) NOT NULL,
                    description TEXT NOT NULL,
                    reminder_at DATETIME NULL,
                    is_completed TINYINT(1) NOT NULL DEFAULT 0,
                    reminder_notified TINYINT(1) NOT NULL DEFAULT 0,
                    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                        ON UPDATE CURRENT_TIMESTAMP,
                    PRIMARY KEY (id),
                    INDEX idx_task_status (is_completed),
                    INDEX idx_task_reminder (reminder_at, reminder_notified)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
                  COLLATE=utf8mb4_unicode_ci;";

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }

            public async Task<List<CyberTask>> GetAllAsync()
            {
                const string sql = @"
                SELECT id, title, description, reminder_at, is_completed,
                       reminder_notified, created_at, updated_at
                FROM cyber_tasks
                ORDER BY is_completed ASC,
                         CASE WHEN reminder_at IS NULL THEN 1 ELSE 0 END,
                         reminder_at ASC,
                         created_at DESC;";

                List<CyberTask> tasks = new List<CyberTask>();

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    await connection.OpenAsync();
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tasks.Add(MapTask(reader));
                        }
                    }
                }

                return tasks;
            }

            public async Task<int> AddAsync(CyberTask task)
            {
                const string sql = @"
                INSERT INTO cyber_tasks
                    (title, description, reminder_at, is_completed, reminder_notified)
                VALUES
                    (@title, @description, @reminderAt, 0, 0);";

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    AddTaskParameters(command, task);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    return Convert.ToInt32(command.LastInsertedId);
                }
            }

            public async Task UpdateAsync(CyberTask task)
            {
                const string sql = @"
                UPDATE cyber_tasks
                SET title = @title,
                    description = @description,
                    reminder_at = @reminderAt,
                    reminder_notified = CASE
                        WHEN @reminderAt IS NULL THEN 0
                        WHEN reminder_at IS NULL OR reminder_at <> @reminderAt THEN 0
                        ELSE reminder_notified
                    END
                WHERE id = @id;";

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    AddTaskParameters(command, task);
                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = task.Id;
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }

            public async Task<bool> SetCompletionAsync(int taskId, bool isCompleted)
            {
                const string sql = @"
                UPDATE cyber_tasks
                SET is_completed = @isCompleted
                WHERE id = @id;";

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.Add("@isCompleted", MySqlDbType.Byte).Value =
                        isCompleted ? 1 : 0;
                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = taskId;
                    await connection.OpenAsync();
                    return await command.ExecuteNonQueryAsync() > 0;
                }
            }

            public async Task DeleteAsync(int taskId)
            {
                const string sql = "DELETE FROM cyber_tasks WHERE id = @id;";

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = taskId;
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }

            public async Task<List<CyberTask>> GetDueRemindersAsync()
            {
                const string sql = @"
                SELECT id, title, description, reminder_at, is_completed,
                       reminder_notified, created_at, updated_at
                FROM cyber_tasks
                WHERE is_completed = 0
                  AND reminder_notified = 0
                  AND reminder_at IS NOT NULL
                  AND reminder_at <= @now
                ORDER BY reminder_at ASC;";

                List<CyberTask> tasks = new List<CyberTask>();

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.Add("@now", MySqlDbType.DateTime).Value = DateTime.Now;
                    await connection.OpenAsync();
                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tasks.Add(MapTask(reader));
                        }
                    }
                }

                return tasks;
            }

            public async Task MarkReminderNotifiedAsync(int taskId)
            {
                const string sql = @"
                UPDATE cyber_tasks
                SET reminder_notified = 1
                WHERE id = @id;";

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.Add("@id", MySqlDbType.Int32).Value = taskId;
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }

            private static void AddTaskParameters(MySqlCommand command, CyberTask task)
            {
                command.Parameters.Add("@title", MySqlDbType.VarChar, 150).Value = task.Title;
                command.Parameters.Add("@description", MySqlDbType.Text).Value =
                    task.Description ?? string.Empty;
                command.Parameters.Add("@reminderAt", MySqlDbType.DateTime).Value =
                    task.ReminderAt.HasValue
                        ? (object)task.ReminderAt.Value
                        : DBNull.Value;
            }

            private static CyberTask MapTask(DbDataReader reader)
            {
                int reminderOrdinal = reader.GetOrdinal("reminder_at");

                return new CyberTask
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Title = Convert.ToString(reader["title"]),
                    Description = Convert.ToString(reader["description"]),
                    ReminderAt = reader.IsDBNull(reminderOrdinal)
                        ? (DateTime?)null
                        : Convert.ToDateTime(reader["reminder_at"]),
                    IsCompleted = Convert.ToBoolean(reader["is_completed"]),
                    ReminderNotified = Convert.ToBoolean(reader["reminder_notified"]),
                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                    UpdatedAt = Convert.ToDateTime(reader["updated_at"])
                };
            }
        }
    }
