using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace Tetris.Model
{
    public class Highscores
    {
        #region Fields/Properties

        private static Highscores _instance;
        private readonly List<Score> _scores;

        private static string SerializationPath
        {
            get { return Path.Combine(((App) Application.Current).SerializationPath, "scores.dat"); }
        }

        public static Highscores Instance
        {
            get
            {
                _instance = _instance ?? new Highscores();
                return _instance;
            }
        }

        public List<Score> Scores
        {
            get { return _scores; }
        }

        #endregion

        /// <summary>
        /// Recreate a previously serialized highscore list from the file system, or create a new empty one.
        /// </summary>
        private Highscores()
        {
            _scores = new List<Score>();

            #region Deserialize the list of highscores if it already exists

            if (File.Exists(SerializationPath))
            {
                var formatter = new BinaryFormatter();
                var stream = new FileStream(SerializationPath, FileMode.Open);
                _scores = (List<Score>) formatter.Deserialize(stream);
                stream.Close();
            }

            #endregion
        }

        #region Methods

        /// <summary>
        /// Adds a new highscore to the list.
        /// </summary>
        /// <param name="points">The value of the new score.</param>
        /// <param name="name">The players name.</param>
        public void Add(int points, string name)
        {
            if (CheckScore(points))
            {
                #region Remove the lowest score if there are 10

                if (_scores.Count == 10)
                    _scores.Remove(_scores.OrderBy(s => s.Points).First());

                #endregion

                _scores.Add(new Score(points, name, _scores));

                #region Serialize the complete Highscores-list

                try
                {
                    var stream = new FileStream(SerializationPath, FileMode.Create);
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, _scores);
                    stream.Close();
                }
                catch (Exception)
                {
                }

                #endregion
            }
        }

        /// <summary>
        /// Determines whether the specified score is in the top ten.
        /// </summary>
        /// <param name="points">The score to check.</param>
        /// <returns>True if the score is high enough to be in the top ten.</returns>
        public bool CheckScore(int points)
        {
            if ((_scores.Count(s => s.Points >= points) < 10))
                return true;
            else
                return false;
        }

        #endregion

        [Serializable]
        public class Score
        {
            public int Nr
            {
                get
                {
                    if (_list != null) 
                        return _list.OrderByDescending(s => s.Points).ToList().IndexOf(this) + 1;
                    else
                        return 0;
                }
            }
            public int Points { get; set; }
            public string Player { get; set; }

            private readonly List<Score> _list;

            public Score(int points, string name, List<Score> list)
            {
                Points = points;
                Player = name;
                _list = list;
            }
        }
    }
}