using ChuckNorris.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChuckNorris.WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Result joke;
        private int jokesCount;
        private List<string> categories;
        
        public MainWindow()
        {
            InitializeComponent();
            
            CompleteCategories();
            ShowRandomJokes();
        }

        #region Вывести категории шуток
        private async void CompleteCategories()
        {
            string response = await GetResponse("https://matchilling-chuck-norris-jokes-v1.p.rapidapi.com/jokes/categories");
            categories = JsonConvert.DeserializeObject<List<string>>(response);
            categoriesListBox.ItemsSource = categories;
            
        }
        #endregion

        #region Вывести рандомные шутки
        private async void ShowRandomJokes()
        {
            jokesCount = int.Parse(jokesCountTextBox.Text);

            string response;

            for (int i = 0; i < jokesCount; i++)
            {
                response = await GetResponse("https://matchilling-chuck-norris-jokes-v1.p.rapidapi.com/jokes/random");
                if (response.Length > 0)
                {
                    joke = JsonConvert.DeserializeObject<Result>(response);
                    richTextBox.AppendText($"{i + 1}) {joke.Value}\n");
                }
                else
                    break;
            }
        }
        #endregion

        #region Получить ответ к запросу
        private Task<string> GetResponse(string url)
        {
            return Task.Run(() =>
            {
                string response = "";
                try
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                    httpWebRequest.Accept = "application/json";
                    httpWebRequest.Headers["X-RapidAPI-Key"] = "a0ffdc746bmsh1d3fccdb8bdebd4p105957jsncf7f0f18ddad";
                    httpWebRequest.Headers["X-RapidAPI-Host"] = "matchilling-chuck-norris-jokes-v1.p.rapidapi.com";
                    httpWebRequest.Method = "GET";
                    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (StreamReader reader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        response = reader.ReadToEnd();
                    }
                }
                catch (WebException exception)
                {
                    MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return response;
            });
        }
        #endregion

        #region Изменение текста текстбокса при клике мышью
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchTextBox.Text == "Search request...")
            {
                TextBox textBox = (TextBox)sender;
                textBox.Text = string.Empty;
            }
        }
        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (searchTextBox.Text == "")
            {
                TextBox textBox = (TextBox)sender;
                textBox.Text = "Search request...";
            }
        }
        #endregion

        #region Двойной клик или нажатие Enter по категории
        private async void CategoriesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(int.TryParse(jokesCountTextBox.Text, out jokesCount))
            {
                var category = categories[categoriesListBox.SelectedIndex];
                string url = "https://matchilling-chuck-norris-jokes-v1.p.rapidapi.com/jokes/random?category=" + category;

                richTextBox.Document.Blocks.Clear();

                string response;

                for (int i = 0; i < jokesCount; i++)
                {
                    response = await GetResponse(url);
                    if (response.Length > 0)
                    {
                        joke = JsonConvert.DeserializeObject<Result>(response);
                        Dispatcher.Invoke((Action)delegate
                        {
                            richTextBox.AppendText($"{i + 1}) {joke.Value} \n");
                        });
                    }
                }
            }
            else
                MessageBox.Show("Enter jokes count on page in numeric value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void CategoriesListBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CategoriesListBox_MouseDoubleClick(this, null);
        }
        #endregion

        #region Нажатие Enter после ввода текстового запроса
        private async void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && searchTextBox.Text.Length > 0)
            {
                string url = "https://matchilling-chuck-norris-jokes-v1.p.rapidapi.com/jokes/search?query=" + searchTextBox.Text;

                string response = await GetResponse(url);
                var query = JsonConvert.DeserializeObject<Query>(response);

                string jokes = "";
                int counter = 0;
                foreach (var joke in query.Result)
                {
                    jokes += $"{++counter}) " + joke.Value + "\n";
                }

                if (jokes.Length > 0)
                {
                    richTextBox.Document.Blocks.Clear();
                    richTextBox.AppendText(jokes);
                }
                else
                {
                    MessageBox.Show("No matches found for current search", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    searchTextBox.Text = "";
                    return;
                }
            }
        }
        #endregion
    }
}
