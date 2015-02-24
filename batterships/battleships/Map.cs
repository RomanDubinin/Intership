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
		public Vector ShipLocation { get; private set; }
		public int ShipSize { get; private set; }
		public bool DirectionIsHorizont { get; private set; }
		public HashSet<Vector> AliveCells { get; private set; }
		
		public Ship(Vector shipLocation, int shipSize, bool directionIsHorizont)
		{
			ShipLocation = shipLocation;
			ShipSize = shipSize;
			DirectionIsHorizont = directionIsHorizont;
			AliveCells = new HashSet<Vector>(GetShipCells());
		}

		//todo
		public List<Vector> GetShipCells()
		{
			var shipDirectionVector = DirectionIsHorizont ? new Vector(1, 0) : new Vector(0, 1);
			var shipCells = new List<Vector>();
			for (int i = 0; i < ShipSize; i++)
			{
				var shipCell = shipDirectionVector.Mult(i).Add(ShipLocation);
				shipCells.Add(shipCell);
			}
			return shipCells;
		}

		public bool IsAlive
		{
			get { return AliveCells.Any(); }
		}
		
	}


	/////////////////////////////////////////////////////////////////////////////////////////////////
	/// Карта
	/////////////////////////////////////////////////////////////////////////////////////////////////

	public class Map
	{
		private static CellState[,] StatesMap;
		public static Ship[,] ShipsMap;
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
				if (!CheckBounds(p))
					throw new IndexOutOfRangeException(p + " is not in the map borders");
				return StatesMap[p.X, p.Y];
			}
			private set
			{
				if (!CheckBounds(p))
					throw new IndexOutOfRangeException(p + " is not in the map borders");
				StatesMap[p.X, p.Y] = value;
			}
		}

		///<summary>Помещает корабль длинной i в точку v, смотрящий в направлении d</summary>
		public bool Set(Vector v, int n, bool direction)
		{
			var ship = new Ship(v, n, direction);
			var shipCells = ship.GetShipCells();
			//Если рядом есть непустая клетка, то поместить корабль нельзя!
			if (shipCells.SelectMany(Neighbours).Any(c => this[c] != CellState.Empty)) return false;
			//Если корабль не помещается — тоже нельзя
			if (!shipCells.All(CheckBounds)) return false;

			// Иначе, ставим корабль
			foreach (var cell in shipCells)
				{
					this[cell] = CellState.Ship;
					ShipsMap[cell.X, cell.Y] = ship;
				}
			Ships.Add(ship);
			return true;
		}

		///<summary>Бойтесь все!!!</summary>
		public ShotEffect Badaboom(Vector target)
		{
			var hit = CheckBounds(target) && this[target] == CellState.Ship;
			
			
			if (hit)
			{
				var ship = ShipsMap[target.X, target.Y];
				ship.AliveCells.Remove(target);
				this[target] = CellState.DeadOrWoundedShip;
				return ship.IsAlive ? ShotEffect.Wound : ShotEffect.Kill;
			}


			if (this[target] == CellState.Empty) this[target] = CellState.Miss;
			return ShotEffect.Miss;
		}

		public IEnumerable<Vector> Neighbours(Vector cell)
		{
			return
				from x in new[] {-1, 0, 1} 
				from y in new[] {-1, 0, 1} 
				let c = cell.Add(new Vector(x, y))
				where CheckBounds(c)
				select c;
		}

		public bool CheckBounds(Vector p)
		{
			return p.X >= 0 && 
				   p.X < MapWidth && 
				   p.Y >= 0 && 
				   p.Y < MapHeight;
		}

		public bool HasAliveShips()
		{
			return Ships.Any(s => s.IsAlive);
		}
	}
}