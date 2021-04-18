using System;
using System.Collections.Generic;
using Npgsql;

namespace DVD_rental
{
    public class Database
    {
        private readonly string _path = "Host=localhost;Port=5500;Username=postgres;Password=allocator123;Database=dvdrental";
        private NpgsqlConnection _connection;

        public Database()
        {
            Init();
        }

        public Database(string port, string username, string password, string database)
        {
            _path = $"Host=localhost;Port={port};Username={username};Password={password};Database={database}";
            Init();
        }

        private void Init()
        {
            _connection = new NpgsqlConnection(_path);
            _connection.Open();
        }

        public List<string> GetFilms()
        {
            var res = new List<string>();
            var query = "SELECT film.title, film.release_year, language.name, film.length, film.rating FROM film JOIN language ON film.language_id = language.language_id;";
            var cmd = new NpgsqlCommand(query, _connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                res.Add((string)reader[0]);
            }
            reader.Dispose();
            return res;
        }

        public List<string> GetLanguages()
        {
            var result = new List<string>();
            var query = "SELECT name FROM language";
            var cmd = new NpgsqlCommand(query, _connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add((string)reader[0]);
            }
            reader.Dispose();
            return result;
        }

        public List<string> GetCategories()
        {
            var query = "SELECT name FROM category";
            var res = new List<string>();
            var cmd = new NpgsqlCommand(query, _connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                res.Add((string) reader[0]);
            }
            reader.Dispose();
            return res;
        }

        public List<string> GetRatings()
        {
            var query = "SELECT rating FROM film GROUP BY rating";
            var res = new List<string>();
            var cmd = new NpgsqlCommand(query, _connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                res.Add((string)reader[0]);
            }
            reader.Dispose();
            return res;
        }

        public List<string> GetFilmsWithActor(string name, string surname)
        {
            var query =
                $"SELECT film.title, film.length, film.rating, language.name, category.name FROM film JOIN film_category ON film.film_id = film_category.film_id JOIN category ON film_category.category_id = category.category_id JOIN language ON film.language_id = language.language_id JOIN film_actor ON film.film_id = film_actor.film_id JOIN actor ON film_actor.actor_id = actor.actor_id WHERE actor.first_name = '{name}' AND actor.last_name = '{surname}';";
            var cmd = new NpgsqlCommand(query, _connection);
            var reader = cmd.ExecuteReader();
            var res = new List<string>();
            while (reader.Read())
            {
                res.Add( reader[0] + "\t" + reader[1] + "\t" + reader[2] + "\t" + reader[3] + "\t" + reader[4]);
            }
            reader.Dispose();
            return res;
        }

        public List<string> GetFilmsWithFilter(string filterQuery)
        {
            var query =
                "SELECT film.title, film.length, film.rating, language.name, category.name FROM film JOIN film_category ON film.film_id = film_category.film_id JOIN category ON film_category.category_id = category.category_id JOIN language ON film.language_id = language.language_id";
            if (filterQuery.Length > 0)
            {
                query += " WHERE " + filterQuery;
            }
            var cmd = new NpgsqlCommand(query, _connection);
            var reader = cmd.ExecuteReader();
            var res = new List<string>();
            while (reader.Read())
            {
                res.Add(reader[0] + "\t" + reader[1] + "\t" + reader[2] + "\t" + reader[3] + "\t" + reader[4]);
            }
            reader.Dispose();
            return res;
        }

        public List<string> GetFilmsWithCost(string title)
        {
            var query = $"SELECT film.title, film.length, film.rating, language.name, category.name FROM film JOIN film_category ON film.film_id = film_category.film_id JOIN category ON film_category.category_id = category.category_id JOIN language ON film.language_id = language.language_id, (SELECT film.replacement_cost AS cost FROM film WHERE film.title = \'{title}\') AS tb WHERE tb.cost = film.replacement_cost;";
            var res = new List<string>();
            var cmd = new NpgsqlCommand(query, _connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                res.Add(reader[0] + "\t" + reader[1] + "\t" + reader[2] + "\t" + reader[3] + "\t" + reader[4]);
            }
            reader.Dispose();
            return res;
        }

        public List<string> GetActorsWithFilter(string filterQuery)
        {
            var query = "SELECT actor.first_name, actor.last_name FROM actor";
            if (filterQuery.Length > 0)
            {
                query += " WHERE " + filterQuery;
            }
            var res = new List<string>();
            var cmd = new NpgsqlCommand(query, _connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                res.Add(reader[0] + "\t" + reader[1]);
            }
            reader.Dispose();
            return res;
        }
    }
}