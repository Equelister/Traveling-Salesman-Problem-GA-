using System;
using System.Collections.Generic;
using System.Linq;

namespace KomiwojazerGA
{
    class Program
    {
        public int NumberOfCities;
        public const int RunTimes = 10;
        public const int PopulationSize = 300;
        //public const int NumberOfIterations = 100000;
        public const int NumberOfEliteRoutes = (int)(PopulationSize*0.20);
        public const double CrossingPropability = 0.9;
        public const double MutationPropability = 0.15;
        public int[,] citiesDistanceArray;

        static void Main(string[] args)
        {
            Console.WriteLine("MM => 6 Semester of Engineering Computer Science. \r\nTravelling Salesman Problem [TSP]\r\n");
            Program p = new Program();

            //DateTime startTime = DateTime.Now;
            p.Run();
            //Console.WriteLine("Run Time [s]: " + DateTime.Now.Subtract(startTime).TotalSeconds);
        }

        public void Run()
        {
            FileManager fm = new FileManager();

            List<string> paths = fm.GetAllFilesPaths();
            int pickedPathID = CreateMenuAndGetPathID(paths);

            if(pickedPathID == -1)
            {
                Console.WriteLine("Exit.");
                return;
            }

            List<CityModel> Cities = fm.ReadCitiesFromPath(paths[pickedPathID], out NumberOfCities);
            if (Cities.Count <= 0)
            {
                Console.WriteLine("Error => No Cities.");
                return;
            }

            String fileNameSave = paths[pickedPathID].Substring(paths[pickedPathID].LastIndexOf('\\') + 1, (paths[pickedPathID].Length - paths[pickedPathID].LastIndexOf('\\')) - 1 - 4);

            citiesDistanceArray = new int[NumberOfCities, NumberOfCities];
            CalculateCitiesDistances(ref citiesDistanceArray, Cities);

            DateTime startTime = DateTime.Now;

            for (int run = 0; run < RunTimes; run++)                        //Ilość uruchomień programu
            {                
                RouteModel[] Routes = CreateRandomRoutes(citiesDistanceArray, Cities);
                SortRoutesASC(ref Routes);                                  //Sortowanie szybsze od LINQ praktycznie tylko w pierwszym sortowaniu

                while(DateTime.Now.Subtract(startTime).TotalSeconds < 299)          //for (int i = 1; i < NumberOfIterations; i++)                //While(DateTime.Now.Subtract(startTime).TotalSeconds < 290)
                {
                    Routes = Crossing(Routes);                              //Order crossover (OX, Davis, 1985)
                    Mutation(ref Routes);                                   //Scramble Mutation
                    Routes = Selection(ref Routes);                         //Metoda Rankingowa

                    Routes = Routes.OrderBy(x => x.Distance).ToArray();     //Sortowanie we wszystkich pozostałych iteracjach oprócz 1 praktycznie 2x szybsze
                }
                Console.WriteLine(DateTime.Now.Subtract(startTime).TotalSeconds);
                fm.SaveToFile(Routes[0], fileNameSave);                                   // wybor najlepszego (o indexie [0]) i do pliku
                startTime = DateTime.Now;
            }

            /*foreach (var route in Routes)
            {
                Console.WriteLine(route.Distance);
            }*/
        }

        /// <summary>
        /// Selekcja do nowej populacji metodą rankingową z elitaryzmem
        /// </summary>
        /// <param name="routes"></param>
        /// <returns></returns>
        private RouteModel[] Selection(ref RouteModel[] routes)
        {
            RouteModel[] routesChild = new RouteModel[PopulationSize];
            Random rand = new Random();

            for(int i = 0; i < NumberOfEliteRoutes; i++)
            {
                routesChild[i] = routes[i];
            }

            routes = routes.OrderByDescending(x => x.Distance).ToArray();   //Ulozone malejąco, by trasa o najmniejszym dystansie miała jak najwięcej punktów rankingowych
                                                                            //(największe parwdopodobieństwo przejścia dalej)
            int[] rankArr = new int[PopulationSize];                        //Tablica z punktami {1,2,3,4,5...,k}
            int sum = 0;

            for(int i = 1; i<= PopulationSize; i++)
            {
                rankArr[i - 1] = i;
                sum += i;
            }

            int placeToPut = NumberOfEliteRoutes;
            int currentPosition = 0;
            for (int index = 0; placeToPut < PopulationSize; placeToPut++)      //Pierwsze X miejsc zajęte już przez elitę, uzupełnianie pozostałych
            {
                int i = 0;
                int randNumber = rand.Next(0, sum);
                
                foreach (var elem in rankArr)
                {
                    if(i <= randNumber && randNumber < i+rankArr[currentPosition])     //Jeśli losowa liczba trafi w przedział danego osobnika, to przechodzi on dalej
                    {
                        routesChild[placeToPut] = routes[currentPosition];
                        currentPosition = 0;
                        index++;
                        break;
                    }else
                    {
                        i += rankArr[currentPosition];                                  //Zmiana sprawdzanego przedziału
                        currentPosition++;
                    }
                }
            }

            return routesChild;
        }

        /// <summary>
        /// Mutuje osobniki z tablicy <paramref name="routes"/> tworząc nowych osobników do następnej populacji (Operator Scramble Mutation)
        /// </summary>
        /// <param name="routes">Tablica obiektów typu 'RouteModel' zawierająca trasy po krzyżowaniu, przed mutacją</param>
        private void Mutation(ref RouteModel[] routes)
        {
            Random rand = new Random();
            int i = NumberOfEliteRoutes;    //Pominięcie elity
      
            for(; i< PopulationSize; i++)
            {
                if(rand.NextDouble() < MutationPropability)
                {
                    int numberToSwap = (int)(NumberOfCities*0.2);                                  //Tyle ID miast będzie zmienianych ze sobą
                    int[] randomPickedIDArr = new int[numberToSwap];
                    List<int> IDOcurrencesList = new List<int>();

                    for (int j = 0; j < numberToSwap; j++)
                    {                                                       //Losowanie ID miasta
                        int randNumber = rand.Next(0, NumberOfCities) +1;    // '+1' bo ID miast sa od 1-127
                        if (IDOcurrencesList.Contains(randNumber) == false)
                        {
                            IDOcurrencesList.Add(randNumber);               //Zapisanie wylosowanego ID miasta do listy kontrolnej czy ID nie zostało już wylosowane
                            randomPickedIDArr[j] = randNumber;
                        }else
                        {
                            j--;
                        }
                    }

                    randomPickedIDArr = randomPickedIDArr.OrderBy(x => rand.Next()).ToArray();          

                    for(int j = 0; j<numberToSwap; j++)
                    {
                        int index = Array.IndexOf(routes[i].CitiesIDOrderArr, IDOcurrencesList[j]);     //Pobranie miejsca (index'u) w tablicy, gdzie znajduje się wybrane ID miasta
                        routes[i].CitiesIDOrderArr[index] = randomPickedIDArr[j];                       //Podstawienie nowego ID miasta w pobrany index
                    }
                    routes[i].setDistance(citiesDistanceArray);                                         //Obliczenie dystansu po zmianie kolejności ID miast
                }
            }
        }

        private void SortRoutesASC(ref RouteModel[] routes)
        {
            //DateTime startTime = DateTime.Now;
            MergeSort(routes, 0, routes.Length - 1);
            //Console.WriteLine("Merge Sort Time [ms]: " + DateTime.Now.Subtract(startTime).TotalMilliseconds);
        }

        private void MergeSort(RouteModel[] routes, int left, int right)
        {
            if (left < right)
            {
                int middle = (left + right) / 2;
                MergeSort(routes, left, middle);
                MergeSort(routes, middle + 1, right);
                Merge(routes, left, middle, right);
            }
        }

        private void Merge(RouteModel[] routes, int left, int middle, int right)
        {
            RouteModel[] leftArray = new RouteModel[middle - left + 1];
            RouteModel[] rightArray = new RouteModel[right - middle];

            Array.Copy(routes, left, leftArray, 0, middle - left + 1);
            Array.Copy(routes, middle + 1, rightArray, 0, right - middle);

            int i = 0;
            int j = 0;
            for (int k = left; k < right + 1; k++)
            {
                if (i == leftArray.Length)
                {
                    routes[k] = rightArray[j];
                    j++;
                }
                else if (j == rightArray.Length)
                {
                    routes[k] = leftArray[i];
                    i++;
                }
                else if (leftArray[i].Distance <= rightArray[j].Distance)
                {
                    routes[k] = leftArray[i];
                    i++;
                }
                else
                {
                    routes[k] = rightArray[j];
                    j++;
                }
            }
        }

        /// <summary>
        /// Losuje ID (rodzica) trasy do krzyżowania i sprawdza czy dane ID (rodzic) już się nie krzyżował, jeśli krzyżował to próba wylosowania innego ID 
        /// </summary>
        /// <param name="IDOcurrencesList">Lista do sprawdzania i przechowywania ID (rodziców), które brały już udział w krzyżowaniu</param>
        /// <returns>Wylosowane ID trasy</returns>
        public int CrossingPickRouteIDRandom(List<int> IDOcurrencesList)
        {
            Random rand = new Random();
            int randID = rand.Next(0, PopulationSize);
            for (int j = 0; j < IDOcurrencesList.Count; j++)
            {
                if (randID == IDOcurrencesList[j])
                {
                    randID = rand.Next(0, PopulationSize);                  //Sprawdzenie czy element się łączył czy jeszcze nie
                    j = -1;                                                 //Jesli byl to losowanie nowego id i reset petli (j=-1)
                }
            }
            IDOcurrencesList.Add(randID);
            return randID;
        }

        /// <summary>
        /// Krzyżuje osobniki z tablicy <paramref name="routes"/> tworząc nowych osobników do następnej populacji (Order Crossover OX, Davis, 1985)
        /// </summary>
        /// <param name="routes">Tablica obiektów typu 'RouteModel' zawierająca trasy poprzedniej populacji</param>
        /// <returns>Nowa tablicę obiektów typu 'RouteModel' zawierającą Elitę z poprzedniej populacji oraz potomków</returns>
        private RouteModel[] Crossing(RouteModel[] routes)
        {
            RouteModel[] routesChild = new RouteModel[PopulationSize];

            int currentIndex = 0;
            for(; currentIndex < NumberOfEliteRoutes; currentIndex++)
            {
                routesChild[currentIndex] = routes[currentIndex];       //Wypełnienie tablicy następnej populacji elitą z populacji bieżącej
            }                                                           //Elita może normalnie się krzyżować

            Random rand = new Random();
            List<int> IDOcurrencesList = new List<int>();
            for (; currentIndex < PopulationSize; )
            {
                int firstID = CrossingPickRouteIDRandom(IDOcurrencesList);
                if ((PopulationSize % 2 == 1) && (IDOcurrencesList.Count == PopulationSize))
                {
                    routesChild[currentIndex++] = routes[firstID];
                    if (currentIndex >= PopulationSize)                                 //Jeśli tablica wypełniona => break
                    {
                        break;
                    }
                }
                int secondID = CrossingPickRouteIDRandom(IDOcurrencesList);

                double w_lb_psl = rand.NextDouble();
                if (w_lb_psl < CrossingPropability)
                {
                    int leftPoint, rightPoint;
                    do
                    {
                        leftPoint = rand.Next(0, NumberOfCities);      //Losowanie przedziału przecięcia
                        rightPoint = rand.Next(0, NumberOfCities);

                        if (leftPoint > rightPoint)
                        {
                            int temp = leftPoint;                     //Ustawienie, by leftPoint był zawsze mniejszy od rightPoint
                            leftPoint = rightPoint;
                            rightPoint = temp;
                        }
                    } while (leftPoint == rightPoint);

                    routesChild[currentIndex++] = CrossTwoParentsIntoOneChild(leftPoint, rightPoint, firstID, secondID, routes);    //Tworzenie pierwszego potomka
                    if (currentIndex >= PopulationSize)                                                                             //Jeśli tablica wypełniona => break
                    {
                        break;
                    }
                    routesChild[currentIndex++] = CrossTwoParentsIntoOneChild(leftPoint, rightPoint, secondID, firstID, routes);    //Tworzenie drugiego potomka
                    if (currentIndex >= PopulationSize)                                                                             //Jeśli tablica wypełniona => break
                    {
                        break;
                    }
                }else
                {
                    routesChild[currentIndex++] = routes[firstID];                      //Jeśli wylosowana liczba pseudolosowa >= prawdopodobieństwa krzyżowania, to 
                    if(currentIndex >= PopulationSize)                                  //przepisz wylosowane osobniki do nowej populacji
                    {                                                                   //Jeśli tablica wypełniona => break
                        break;
                    }
                    routesChild[currentIndex++] = routes[secondID];
                    if (currentIndex >= PopulationSize)                                 //Jeśli tablica wypełniona => break
                    {
                        break;
                    }
                }
            }
            return routesChild;
        }

        /// <summary>
        /// Krzyżuje ze sobą dwóch rodziców w celu wytworzenia jednego potomka klasy 'RouteModel'
        /// </summary>
        /// <param name="leftPoint">Lewy punkt przedziału przecięcia</param>
        /// <param name="rightPoint">Prawy punkt przedziału przecięcia</param>
        /// <param name="firstID">ID pierwszego rodzica</param>
        /// <param name="secondID">ID drugiego rodzica</param>
        /// <param name="routes">Tablica obiektów typu 'RouteModel' zawierająca trasy poprzedniej populacji</param>
        /// <returns>Potomka powstałego z krzyżowania klasy 'RouteModel'</returns>
        public RouteModel CrossTwoParentsIntoOneChild(int leftPoint, int rightPoint, int firstID, int secondID, RouteModel[] routes)
        {
            int[] tempIDOrderArr = new int[(rightPoint - leftPoint) + 1];           //Mniejsza tablica z zawartością przedziału przecięcia dla optymalizacji przeszukiwania tablicy
            int tempIndex = 0;                                                      //(Zamiast przeszukiwać zawsze tablice główną o wielkości 'NumberOfCities'
                                                                                    //przeszukujemy zawsze część tej tablicy, na której nam zależy (od leftPoint do rightPoint) = 'tempIDOrderArr')
            RouteModel route = new RouteModel(NumberOfCities);
            for (int i = leftPoint; i <= rightPoint; i++)
            {
                route.CitiesIDOrderArr[i] = routes[firstID].CitiesIDOrderArr[i];
                tempIDOrderArr[tempIndex++] = routes[firstID].CitiesIDOrderArr[i];
            }

            int index = 0;
            int placeToPut = rightPoint + 1;
            for (index = 0; index < route.CitiesIDOrderArr.Length - rightPoint; index++)
            {
                if (placeToPut >= route.CitiesIDOrderArr.Length)        //Jeśli planowane miejsce wsadzenia kolejnej danej przekraczca długość tablicy, to ustawia się planowane miejsce na początek tej tablicy 
                {                                                       //Gdyż wsadzenia nie zaczynają się od początku, tylko np. od połowy
                    placeToPut = 0;
                }

                if (tempIDOrderArr.Contains(routes[secondID].CitiesIDOrderArr[index]) == false)         //Jeśli planowany do wsadzenia ID miasta nie był już wsadzony do tablicy w przedziale przecięcia
                {                                                                                       //to wsadż do tablicy i przesuń miejsce do wsadzenia o 1 w przód
                    route.CitiesIDOrderArr[placeToPut] = routes[secondID].CitiesIDOrderArr[index];
                    placeToPut++;
                }
            }

            for (; index < route.CitiesIDOrderArr.Length; index++)
            {
                if (placeToPut >= route.CitiesIDOrderArr.Length)
                {
                    placeToPut = 0;
                }

                if (tempIDOrderArr.Contains(routes[secondID].CitiesIDOrderArr[index]) == false)
                {
                    route.CitiesIDOrderArr[placeToPut] = routes[secondID].CitiesIDOrderArr[index];
                    placeToPut++;
                }
            }
            route.setDistance(citiesDistanceArray);
            return route;
        }


        /// <summary>
        /// Tworzy menu w konsoli i oczekuje wybrania od użytkownika numeru pliku do odczytania
        /// </summary>
        /// <param name="paths">Lista ścieżek do plików w obecnym folderze</param>
        /// <returns>
        /// Wybrany numer pliku
        /// </returns>
        private int CreateMenuAndGetPathID(List<string> paths)
        {
            List<string> shortPaths = new List<string>();

            foreach (var path in paths)
            {
                shortPaths.Add(path.Substring(path.LastIndexOf('\\')+1, (path.Length - path.LastIndexOf('\\'))-1));
            }

            int index = 0;
            foreach(var shortpath in shortPaths)
            {
                Console.WriteLine(index + ". " + shortpath);
                index++;
            }
            Console.WriteLine("\r\n'-1'. to Exit.\r\n");

            bool result = false;
            int userInputNumber;
            do
            {
                Console.Write("Select File for the Algorythm: ");
                string userinput = Console.ReadLine();
                result = Int32.TryParse(userinput, out userInputNumber);
            } while ((result == false) || (userInputNumber < -1 || userInputNumber > index-1));

            return userInputNumber;
        }

        /// <summary>
        /// Tworzy macierz dla odległości pomiędzy miastami
        /// </summary>
        /// <param name="citiesDistanceArray">Macierz z odległościami pomiędzy poszczególnymi miastami</param>
        /// <param name="cities">Lista obiektów typu 'CityModel'</param>
        private void CalculateCitiesDistances(ref int[,] citiesDistanceArray, List<CityModel> cities)
        {
            for(int i = 0; i<NumberOfCities; i++)
            {
                for(int j=0; j<NumberOfCities; j++)
                {
                    int distance = Math.Abs(cities[i].X - cities[j].X) + Math.Abs(cities[i].Y - cities[j].Y);
                    citiesDistanceArray[i, j] = distance;
                }
            }
        }

        /// <summary>
        /// Tworzy początkową 'populację' tras
        /// </summary>
        /// <param name="citiesDistanceArray">Macierz z odległościami pomiędzy poszczególnymi miastami</param>
        /// <param name="cities">Lista obiektów typu 'CityModel'</param>
        /// <returns>
        /// Tablicę obiektów typu 'RouteModel'
        /// </returns>
        private RouteModel[] CreateRandomRoutes(int[,] citiesDistanceArray, List<CityModel> cities)
        {
            RouteModel[] routes = new RouteModel[PopulationSize];

            for(int i = 0; i< PopulationSize; i++)
            {
                RouteModel route = new RouteModel(citiesDistanceArray, cities);
                routes[i] = route;
            }

            return routes;
        }
    }
}
