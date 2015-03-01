// Автор: Павел Егоров
// Дата: 28.12.2015

using System;
using System.Collections.Generic;
using System.Linq;

namespace battleships
{
	public enum CellState
	{
		Empty,
		Ship,
		DeadOrWoundedShip,
		Miss
	}

	public enum ShotEffect
	{
		Miss,
		Wound,
		Kill
	}

	public class Ship
	{
		public Vector Location { get; private set; }
		public int Size { get; private set; }
		public bool DirectionIsHorizont { get; private set; }
		public HashSet<Vector> AliveCells { get; private set; }
		
		public Ship(Vector location, int size, bool directionIsHorizont)
		{
			Location = location;
			Size = size;
			DirectionIsHorizont = directionIsHorizont;
			AliveCells = new HashSet<Vector>(GetShipCells());
		}

		public List<Vector> GetShipCells()
		{
			var shipDirectionVector = DirectionIsHorizont ? new Vector(1, 0) : new Vector(0, 1);
			var shipCells = new List<Vector>();
			for (var i = 0; i < Size; i++)
			{
				var shipCell = shipDirectionVector.Mult(i).Add(Location);
				shipCells.Add(shipCell);
			}
			return shipCells;
		}

		public bool IsAlive
		{
			get { return AliveCells.Any(); }
		}
		
	}


	public class Map
	{
		private readonly CellState[,] StatesMap;
		public Ship[,] ShipsMap;
		public List<Ship> Ships = new List<Ship>();
		public int MapWidth { get; private set; }
		public int MapHeight { get; private set; }

		public Map(int mapWidth, int mapHeight)
		{
			MapWidth = mapWidth;
			MapHeight = mapHeight;
			StatesMap = new CellState[mapWidth, mapHeight];
			ShipsMap = new Ship[mapWidth, mapHeight];
		}

		public CellState this[Vector p]
		{
			get
			{
				if (!InMapBounds(p))
					throw new IndexOutOfRangeException(string.Format("{0} is not in the map borders", p));
				return StatesMap[p.X, p.Y];
			}
			private set
			{
				if (!InMapBounds(p))
					throw new IndexOutOfRangeException(string.Format("{0} is not in the map borders", p));
				StatesMap[p.X, p.Y] = value;
			}
		}

		public bool Set(Vector shipLocation, int shipSize, bool directionIsHorizont)
		{
			var ship = new Ship(shipLocation, shipSize, directionIsHorizont);

			if (!CanSetShip(ship)) 
				return false;

			var shipCells = ship.GetShipCells();
			foreach (var cell in shipCells)
			{
				this[cell] = CellState.Ship;
				ShipsMap[cell.X, cell.Y] = ship;
			}
			Ships.Add(ship);
			return true;
		}

		public bool CanSetShip(Ship ship)
		{
			var shipCells = ship.GetShipCells();

			if (!shipCells.All(InMapBounds))
				return false;
			
			if (shipCells.SelectMany(Neighbours).Any(c => this[c] != CellState.Empty))
				return false;

			return true;
		}

		public ShotEffect Shot(Vector target)
		{
			if (this[target] == CellState.Ship)
			{
				var ship = ShipsMap[target.X, target.Y];
				ship.AliveCells.Remove(target);
				this[target] = CellState.DeadOrWoundedShip;
				return ship.IsAlive ? ShotEffect.Wound : ShotEffect.Kill;
			}

			if (this[target] == CellState.Empty) 
				this[target] = CellState.Miss;

			return ShotEffect.Miss;
		}

		public IEnumerable<Vector> Neighbours(Vector cell)
		{
			foreach (var x in new[] {-1, 0, 1})
				foreach (var y in new[] {-1, 0, 1})
				{
					var c = cell.Add(new Vector(x, y));
					if (InMapBounds(c)) yield return c;
				}
		}

		public bool InMapBounds(Vector point)
		{
			return point.X >= 0 && 
				   point.X < MapWidth && 
				   point.Y >= 0 && 
				   point.Y < MapHeight;
		}

		public bool HasAliveShips()
		{
			return Ships.Any(ship => ship.IsAlive);
		}
	}
}