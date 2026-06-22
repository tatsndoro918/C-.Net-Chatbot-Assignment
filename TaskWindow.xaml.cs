using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Cybersecurity_Awareness_Bot
{
    public partial class TaskWindow : Window
    {
        private readonly TaskRepository _repository;
        private List<CyberTask> _allTasks = new List<CyberTask>();
        private int? _editingTaskId;
        private bool _isLoading;

        public TaskWindow(TaskRepository repository)
        {
            InitializeComponent();
            _repository = repository;
            PopulateReminderTimes();
            ResetEditor();
            Loaded += TaskWindow_Loaded;
        }

        private async void TaskWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTasksAsync();
        }

        private void PopulateReminderTimes()
        {
            ReminderTimeComboBox.Items.Clear();
            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    ReminderTimeComboBox.Items.Add(
                        new TimeSpan(hour, minute, 0).ToString(@"hh\:mm"));
                }
            }

            ReminderTimeComboBox.SelectedItem = "09:00";
        }

        private async Task LoadTasksAsync()
        {
            try
            {
                SetBusy(true, "Loading tasks from MySQL...");
                await _repository.EnsureDatabaseReadyAsync();
                _allTasks = await _repository.GetAllAsync();
                ApplyFilters();
                DatabaseStatusTextBlock.Text =
                    "🟢 MySQL connected — changes are saved automatically";
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
            finally
            {
                SetBusy(false, DatabaseStatusTextBlock.Text);
            }
        }

        private async void SaveTaskButton_Click(object sender, RoutedEventArgs e)
        {
            CyberTask task;
            if (!TryBuildTask(out task))
            {
                return;
            }

            try
            {
                SetBusy(true, _editingTaskId.HasValue
                    ? "Updating task..."
                    : "Adding task...");

                if (_editingTaskId.HasValue)
                {
                    task.Id = _editingTaskId.Value;
                    await _repository.UpdateAsync(task);
                    DatabaseStatusTextBlock.Text = "Task updated successfully.";
                }
                else
                {
                    int id = await _repository.AddAsync(task);
                    DatabaseStatusTextBlock.Text =
                        "Task #" + id + " added successfully.";
                }

                ResetEditor();
                await ReloadTaskListAsync();
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
            finally
            {
                SetBusy(false, DatabaseStatusTextBlock.Text);
            }
        }

        private bool TryBuildTask(out CyberTask task)
        {
            task = null;
            ValidationTextBlock.Text = string.Empty;

            string title = TitleTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                ValidationTextBlock.Text = "Enter a task title before saving.";
                TitleTextBox.Focus();
                return false;
            }

            DateTime? reminder = null;
            if (ReminderCheckBox.IsChecked == true)
            {
                if (!ReminderDatePicker.SelectedDate.HasValue)
                {
                    ValidationTextBlock.Text = "Select a reminder date.";
                    return false;
                }

                TimeSpan time;
                string selectedTime = Convert.ToString(ReminderTimeComboBox.SelectedItem);
                if (!TimeSpan.TryParseExact(selectedTime, @"hh\:mm",
                    CultureInfo.InvariantCulture, out time))
                {
                    ValidationTextBlock.Text = "Select a valid reminder time.";
                    return false;
                }

                reminder = ReminderDatePicker.SelectedDate.Value.Date.Add(time);
                CyberTask existingTask = _editingTaskId.HasValue
                    ? _allTasks.FirstOrDefault(item => item.Id == _editingTaskId.Value)
                    : null;
                bool isUnchangedExistingReminder = existingTask != null &&
                    existingTask.ReminderAt.HasValue &&
                    existingTask.ReminderAt.Value == reminder.Value;

                if (reminder.Value <= DateTime.Now && !isUnchangedExistingReminder)
                {
                    ValidationTextBlock.Text =
                        "The reminder must be set to a future date and time.";
                    return false;
                }
            }

            task = new CyberTask
            {
                Title = title,
                Description = description,
                ReminderAt = reminder
            };
            return true;
        }

        private async void CompletionButton_Click(object sender, RoutedEventArgs e)
        {
            CyberTask selected = TaskDataGrid.SelectedItem as CyberTask;
            if (selected == null)
            {
                ValidationTextBlock.Text = "Select a task first.";
                return;
            }

            try
            {
                bool newStatus = !selected.IsCompleted;
                SetBusy(true, newStatus ? "Completing task..." : "Reopening task...");
                bool updated = await _repository.SetCompletionAsync(selected.Id, newStatus);
                if (!updated)
                {
                    DatabaseStatusTextBlock.Text =
                        "That task no longer exists. The list has been refreshed.";
                    await ReloadTaskListAsync();
                    return;
                }
                DatabaseStatusTextBlock.Text = newStatus
                    ? "Task marked as completed. Great defensive play!"
                    : "Task reopened.";
                ResetEditor();
                await ReloadTaskListAsync();
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
            finally
            {
                SetBusy(false, DatabaseStatusTextBlock.Text);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            CyberTask selected = TaskDataGrid.SelectedItem as CyberTask;
            if (selected == null)
            {
                ValidationTextBlock.Text = "Select a task first.";
                return;
            }

            MessageBoxResult answer = MessageBox.Show(
                "Delete task #" + selected.Id + ": " + selected.Title + "?\n\n" +
                "This cannot be undone.",
                "Confirm task deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                SetBusy(true, "Deleting task...");
                await _repository.DeleteAsync(selected.Id);
                DatabaseStatusTextBlock.Text = "Task deleted.";
                ResetEditor();
                await ReloadTaskListAsync();
            }
            catch (Exception ex)
            {
                ShowDatabaseError(ex);
            }
            finally
            {
                SetBusy(false, DatabaseStatusTextBlock.Text);
            }
        }

        private void TaskDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CyberTask selected = TaskDataGrid.SelectedItem as CyberTask;
            if (selected == null || _isLoading)
            {
                return;
            }

            _editingTaskId = selected.Id;
            TitleTextBox.Text = selected.Title;
            DescriptionTextBox.Text = selected.Description;
            ReminderCheckBox.IsChecked = selected.ReminderAt.HasValue;

            if (selected.ReminderAt.HasValue)
            {
                ReminderDatePicker.SelectedDate = selected.ReminderAt.Value.Date;
                string time = selected.ReminderAt.Value.ToString("HH:mm");
                if (!ReminderTimeComboBox.Items.Contains(time))
                {
                    ReminderTimeComboBox.Items.Add(time);
                }
                ReminderTimeComboBox.SelectedItem = time;
            }

            EditorHeading.Text = "Edit task #" + selected.Id;
            SaveTaskButton.Content = "Update Task";
            CompletionButton.Content = selected.IsCompleted
                ? "Reopen Task"
                : "Mark Complete";
            ValidationTextBlock.Text = string.Empty;
        }

        private void ReminderCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (ReminderDatePicker == null || ReminderTimeComboBox == null)
            {
                return;
            }

            bool enabled = ReminderCheckBox.IsChecked == true;
            ReminderDatePicker.IsEnabled = enabled;
            ReminderTimeComboBox.IsEnabled = enabled;

            if (enabled && !ReminderDatePicker.SelectedDate.HasValue)
            {
                ReminderDatePicker.SelectedDate = DateTime.Today.AddDays(1);
            }
        }

        private void ClearEditorButton_Click(object sender, RoutedEventArgs e)
        {
            ResetEditor();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadTasksAsync();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void StatusFilterComboBox_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (TaskDataGrid == null || SearchTextBox == null ||
                StatusFilterComboBox == null)
            {
                return;
            }

            string search = SearchTextBox.Text.Trim();
            int statusIndex = StatusFilterComboBox.SelectedIndex;

            IEnumerable<CyberTask> filtered = _allTasks;
            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(task =>
                    task.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    task.Description.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (statusIndex == 1)
            {
                filtered = filtered.Where(task => !task.IsCompleted);
            }
            else if (statusIndex == 2)
            {
                filtered = filtered.Where(task => task.IsCompleted);
            }

            List<CyberTask> result = filtered.ToList();
            TaskDataGrid.ItemsSource = result;
            TaskCountTextBlock.Text = result.Count + " shown • " +
                _allTasks.Count(task => !task.IsCompleted) + " open • " +
                _allTasks.Count(task => task.IsCompleted) + " completed";
        }

        private async Task ReloadTaskListAsync()
        {
            _allTasks = await _repository.GetAllAsync();
            ApplyFilters();
        }

        private void ResetEditor()
        {
            _editingTaskId = null;
            if (TitleTextBox == null)
            {
                return;
            }

            TaskDataGrid.SelectedItem = null;
            TitleTextBox.Clear();
            DescriptionTextBox.Clear();
            ReminderCheckBox.IsChecked = false;
            ReminderDatePicker.SelectedDate = DateTime.Today.AddDays(1);
            ReminderTimeComboBox.SelectedItem = "09:00";
            EditorHeading.Text = "Create a new task";
            SaveTaskButton.Content = "Add Task";
            CompletionButton.Content = "Mark Complete";
            ValidationTextBlock.Text = string.Empty;
            TitleTextBox.Focus();
        }

        private void SetBusy(bool busy, string status)
        {
            _isLoading = busy;
            SaveTaskButton.IsEnabled = !busy;
            TaskDataGrid.IsEnabled = !busy;
            DatabaseStatusTextBlock.Text = status;
        }

        private void ShowDatabaseError(Exception exception)
        {
            string technicalMessage = exception.Message;
            DatabaseStatusTextBlock.Text = "🔴 MySQL connection unavailable";
            ValidationTextBlock.Text =
                "Tasks could not be loaded. Check that MySQL is running and configured.";

            MessageBox.Show(
                "The Task Assistant could not connect to MySQL.\n\n" +
                "1. Install MySQL Community Server and MySQL Workbench.\n" +
                "2. Run database\\setup.sql in MySQL Workbench.\n" +
                "3. Check CyberBotDatabase in App.config.\n\n" +
                "Technical detail: " + technicalMessage,
                "Task database unavailable",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
