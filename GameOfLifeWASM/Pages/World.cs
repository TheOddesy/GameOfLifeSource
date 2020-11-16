using System;
using System.Threading.Tasks;

namespace GameOfLifeWASM.Pages

{
    public class World
    {
        private readonly int _sizeX;
        private readonly int _sizeY;
        private readonly bool[][,] _cells;
        private int _refreshDelay;

        public World(int sizeX, int sizeY)
        {
            _sizeX = sizeX;
            _sizeY = sizeY;

            this.Generation = 0;

            _cells = new[]
            {
                new bool[sizeX, sizeY],
                new bool[sizeX, sizeY]
            };

            Reset();
            SetRefreshDelay(100);
        }

        public void Reset()
        {
            this.Generation = 0;

            var rand = new Random();

            for (var y = 0; y != _sizeY; ++y)
            {
                for (var x = 0; x != _sizeX; ++x)
                {
                    _cells[0][x, y] = rand.NextDouble() > .6;
                    _cells[1][x, y] = rand.NextDouble() > .6;
                }
            }
        }

        public void EmptyPage()
        {
            this.Generation = 0;
            this.Population = 0;

            for (var y = 0; y != _sizeY; ++y)
            {
                for (var x = 0; x != _sizeX; ++x)
                {
                    _cells[0][x, y] = false;
                    _cells[1][x, y] = false;
                }
            }
        }

        public void StepOnce()
        {
            Step();
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (!Paused)
                    {
                        Step();
                        NotifyStateChanged();
                    }
                    await Task.Delay(_refreshDelay);
                }
            });
        }

        public void TogglePause()
        {
            Paused = !Paused;
        }

        public void SetRefreshDelay(int delay)
        {
            _refreshDelay = delay;
        }

        private int IsNeighborAlive(int Index, int proposedX, int proposedY)
        {
            var outOfBounds = proposedX < 0 || proposedX >= _sizeX ||
                                  proposedY < 0 || proposedY >= _sizeY;
            return (!outOfBounds)
                ? _cells[Index][proposedX, proposedY] ? 1 : 0
                : 0;
        }

        private void Step()
        {
            var index = this.Generation & 1;
            var thisGenerationsWorld = _cells[index];
            var nextGeneration = _cells[(1 + this.Generation) & 1];

            this.Population = 0;

            for (var y = 0; y != _sizeY; ++y)
            {
                for (var x = 0; x != _sizeX; ++x)
                {
                    var numberOfAliveNeighbours = IsNeighborAlive(index, x - 1, y)
                                            + IsNeighborAlive(index, x - 1, y + 1)
                                            + IsNeighborAlive(index, x, y + 1)
                                            + IsNeighborAlive(index, x + 1, y + 1)
                                            + IsNeighborAlive(index, x + 1, y)
                                            + IsNeighborAlive(index, x + 1, y - 1)
                                            + IsNeighborAlive(index, x, y - 1)
                                            + IsNeighborAlive(index, x - 1, y - 1);

                    var isAliveThisGeneration = thisGenerationsWorld[x, y];

                    var isAliveNextGeneration = (isAliveThisGeneration && (numberOfAliveNeighbours == 2 ||
                        numberOfAliveNeighbours == 3)) || (!isAliveThisGeneration && numberOfAliveNeighbours == 3);


                    nextGeneration[x, y] = isAliveNextGeneration;
                    this.Population += isAliveNextGeneration ? 1 : 0;
                };
            };

            this.Generation++;
        }

        public bool[,] Cells
        {
            get
            {
                var index = this.Generation & 1;
                return _cells[index];
            }
        }

        public int Generation { get; private set; }
        public int Population { get; private set; }
        public bool Paused { get; private set; }
        public event Func<Task> OnChangeAsync;
        private void NotifyStateChanged() => OnChangeAsync?.Invoke();
    }
}
