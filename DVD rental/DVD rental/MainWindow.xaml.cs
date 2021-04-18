using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DVD_rental
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Database _database = new Database();
        private QueryType _query;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ResizeRender(object sender, SizeChangedEventArgs args)
        {
            //Languages filter grid
            LanguagesList.Width = args.NewSize.Width * 0.25;
            //Film filter grid
            FilmTitleInput.Width = args.NewSize.Width * 0.2;
            LanguageSelect.Width = args.NewSize.Width * 0.2;
            FilmLengthFrom.Width = args.NewSize.Width * 0.1;
            FilmLengthTo.Width = args.NewSize.Width * 0.1;
            RatingSelect.Width = args.NewSize.Width * 0.2;
            CategorySelect.Width = args.NewSize.Width * 0.2;
            //Actor filter grid
            ActorFirstNameInput.Width = args.NewSize.Width * 0.2;
            ActorLastNameInput.Width = args.NewSize.Width * 0.2;
        }

        private void HideFilters()
        {
            FilmsFilter.Visibility = Visibility.Hidden;
            LanguagesFilter.Visibility = Visibility.Hidden;
            ActorsFilter.Visibility = Visibility.Hidden;
        }

        private void InputFilmLength(object sender, TextCompositionEventArgs args)
        {
            args.Handled = new Regex("[^0-9]").IsMatch(args.Text);
        }
        
        private void SelectLanguagesClick(object sender, RoutedEventArgs args)
        {
            HideFilters();
            _query = QueryType.Language;
            LanguagesFilter.Visibility = Visibility.Visible;
            LanguagesList.Items.Clear();
            var languages = _database.GetLanguages();
            foreach (var item in languages)
            {
                LanguagesList.Items.Add(new TextBlock() {Text = item});
            }
        }
        
        private void SelectFilmsClick(object sender, RoutedEventArgs args)
        {
            HideFilters();
            _query = QueryType.Film;
            FilmsFilter.Visibility = Visibility.Visible;
            var languages = _database.GetLanguages();
            var categories = _database.GetCategories();
            var ratings = _database.GetRatings();
            LanguageSelect.Items.Clear();
            foreach (var item in languages)
            {
                LanguageSelect.Items.Add(new TextBlock() {Text = item});
            }
            CategorySelect.Items.Clear();
            foreach (var item in categories)
            {
                CategorySelect.Items.Add(new TextBlock() {Text = item});
            }
            RatingSelect.Items.Clear();
            foreach (var item in ratings)
            {
                RatingSelect.Items.Add(new TextBlock() {Text = item});
            }
        }

        private void SelectActorsClick(object sender, RoutedEventArgs args)
        {
            HideFilters();
            _query = QueryType.Actor;
            ActorsFilter.Visibility = Visibility.Visible;
        }

        private void SelectItemFromOutput(object sender, RoutedEventArgs args)
        {
            if (_query == QueryType.Film)
            {
                var from = (ListBoxItem) sender;
                var title = from.Content.ToString().Split('\t')[0];
                var sameFilms = _database.GetFilmsWithCost(title);
                Info.Items.Clear();
                foreach (var film in sameFilms)
                {
                    var item = new ListBoxItem() {Content = film};
                    Info.Items.Add(item);
                }
            }
            else if (_query == QueryType.Actor)
            {
                var from = (ListBoxItem) sender;
                var data = from.Content.ToString().Split('\t');
                var films = _database.GetFilmsWithActor(data[0], data[1]);
                Info.Items.Clear();
                foreach (var film in films)
                {
                    var item = new ListBoxItem() {Content = film};
                    Info.Items.Add(item);
                }
            }
            else if (_query == QueryType.Language)
            {
                var from = (ListBoxItem) sender;
                var language = from.Content.ToString().Split("\t")[3];
                var query = $"language.name = '{language}'";
                var films = _database.GetFilmsWithFilter(query);
                Info.Items.Clear();
                foreach (var film in films)
                {
                    var item = new ListBoxItem() {Content = film};
                    Info.Items.Add(item);
                }
            }
        }
        
        private void AddQuery(object sender, RoutedEventArgs args)
        {
            if (_query == QueryType.Film)
            {
                var title = FilmTitleInput.Text;
                var language = LanguageSelect.SelectedIndex;
                if (!int.TryParse(FilmLengthFrom.Text, out var lenFrom))
                {
                    lenFrom = 0;
                }
                if (!int.TryParse(FilmLengthTo.Text, out var lenTo))
                {
                    lenTo = -1;
                }
                var category = CategorySelect.SelectedIndex;
                var rating = RatingSelect.SelectedIndex;
                var query = "";
                if (title.Length > 0)
                {
                    query += $"film.title LIKE '%{title}%'";
                }
                if (lenTo != -1)
                {
                    if (query.Length > 0)
                    {
                        query += " AND ";
                    }
                    query += $"film.length BETWEEN {lenFrom} AND {lenTo}";
                }
                if (language != -1)
                {
                    if (query.Length > 0)
                    {
                        query += " AND ";
                    }
                    query += $"film.language_id = {language + 1}";
                }
                if (rating != -1)
                {
                    if (query.Length > 0)
                    {
                        query += " AND ";
                    }
                    query += $"film.rating = \'{((TextBlock) RatingSelect.SelectedItem).Text}\'";
                }
                if (category != -1)
                {
                    if (query.Length > 0)
                    {
                        query += " AND ";
                    }
                    query += $"category.category_id = {category + 1}";
                }
                var films = _database.GetFilmsWithFilter(query);
                Output.Items.Clear();
                foreach (var film in films)
                {
                    var item = new ListBoxItem {Content = film};
                    item.Selected += SelectItemFromOutput;
                    Output.Items.Add(item);
                }
            }
            else if (_query == QueryType.Actor)
            {
                var name = ActorFirstNameInput.Text;
                var surname = ActorLastNameInput.Text;
                var query = "";
                if (name.Length > 0)
                {
                    query += $"actor.first_name LIKE '%{name}%'";
                }

                if (surname.Length > 0)
                {
                    if (query.Length > 0)
                    {
                        query += " AND ";
                    }
                    query += $"actor.last_name LIKE '%{surname}%'";
                }
                
                var actors = _database.GetActorsWithFilter(query);
                Output.Items.Clear();
                foreach (var actor in actors)
                {
                    var item = new ListBoxItem() {Content = actor};
                    item.Selected += SelectItemFromOutput;
                    Output.Items.Add(item);
                }
            }
            else if (_query == QueryType.Language)
            {
                int index = LanguagesList.SelectedIndex;
                var query = "";
                if (index != -1)
                {
                    query += $"film.language_id = {index + 1}";
                }
                var films = _database.GetFilmsWithFilter(query);
                Output.Items.Clear();
                foreach (var film in films)
                {
                    var item = new ListBoxItem() {Content = film};
                    item.Selected += SelectItemFromOutput;
                    Output.Items.Add(item);
                }
            }
        }
    }
}