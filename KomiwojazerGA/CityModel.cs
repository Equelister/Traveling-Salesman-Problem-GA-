namespace KomiwojazerGA
{
    /// <summary>
    /// Klasa ukazująca miasto
    /// </summary>
    class CityModel
    {
        public int ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        /// <summary>
        /// Tworzy obiekt klasy CityModel na podstawie podanych parametrów 
        /// </summary>
        /// <param name="newID">ID miasta</param>
        /// <param name="newX">Położenie geograficzne 'X' miasta</param>
        /// <param name="newY">Położenie geograficzne 'Y' miasta</param>
        public CityModel(int newID, int newX, int newY)
        {
            this.ID = newID;
            this.X = newX;
            this.Y = newY;
        }
    }
}
