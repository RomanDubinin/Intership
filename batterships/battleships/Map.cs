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

	public enum ShotEffct
	{
		Miss,
		Wound,
		Kill
	}

	public class Ship
	{
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
		public Vector ShipLocation { get; private set; }
		public int ShipSize { get; private set; }
		public bool DirectionIsHorizont { get; private set; }
		public HashSet<Vector> AliveCells { get; private set; }
	}


	/////////////////////////////////////////////////////////////////////////////////////////////////
	/// Карта
	/////////////////////////////////////////////////////////////////////////////////////////////////

	public class Map
	{
		private static CellState[,] _cellsState;
		public static Ship[,] shipsMap;

		///<summary>Конструктор</summary>
		public Map(int mapWidth, int mapHeight)
		{
			MapWidth = mapWidth;
			MapHeight = mapHeight;
			_cellsState = new CellState[mapWidth, mapHeight];
			shipsMap = new Ship[mapWidth, mapHeight];
		}

		public List<Ship> Ships = new List<Ship>();
		public int MapWidth { get; private set; }
		public int MapHeight { get; private set; }

		public CellState this[Vector p]
		{
			get
			{
				return CheckBounds(p) ? _cellsState[p.X, p.Y] : CellState.Empty; // Благодаря этому трюку иногда можно будет не проверять на выход за пределы поля. 
			}
			private set
			{
				if (!CheckBounds(p))
					throw new IndexOutOfRangeException(p + " is not in the map borders"); // Поможет отлавливать ошибки в коде.
				_cellsState[p.X, p.Y] = value;
			}
		}

		///<summary>Помещает корабль длинной i в точку v, смотрящий в направлении d</summary>
		public bool Set(Vector v, int n, bool direction)
		{
			var ship = new Ship(v, n, direction);
			var shipCells = ship.GetShipCells();
			//Если рядом есть непустая клетка, то поместить корабль нельзя!
			if (shipCells.SelectMany(Near).Any(c => this[c] != CellState.Empty)) return false;
			//Если корабль не помещается — тоже нельзя
			if (!shipCells.All(CheckBounds)) return false;

			// Иначе, ставим корабль
			foreach (var cell in shipCells)
				{
					this[cell] = CellState.Ship;
					shipsMap[cell.X, cell.Y] = ship;
				}
			Ships.Add(ship);
			return true;
		}

		///<summary>Бойтесь все!!!</summary>
		public ShotEffct Badaboom(Vector target)
		{
			var hit = CheckBounds(target) && this[target] == CellState.Ship;
			
			
			if (hit)
			{
				var ship = shipsMap[target.X, target.Y];
				ship.AliveCells.Remove(target);
				this[target] = CellState.DeadOrWoundedShip;
				return ship.IsAlive ? ShotEffct.Wound : ShotEffct.Kill;
			}


			if (this[target] == CellState.Empty) this[target] = CellState.Miss;
			return ShotEffct.Miss;
		}

		///<summary>Окрестность ячейки</summary>
		public IEnumerable<Vector> Near(Vector cell)
		{
			return
				from i in new[] {-1, 0, 1} //x
				from j in new[] {-1, 0, 1} //y
				let c = cell.Add(new Vector(i, j))
				where CheckBounds(c)
				select c;
		}

		///<summary>Проверка на выход за границы</summary>
		public bool CheckBounds(Vector p)
		{
			return p.X >= 0 && p.X < MapWidth && p.Y >= 0 && p.Y < MapHeight;
		}
		
		///<summary>Есть ли хоть одна живая клетка</summary>
		public bool HasAliveShips()
		{
			for (int index = 0; index < Ships.Count; index++)
			{
				var s = Ships[index];
				if (s.IsAlive) return true;
			}
			return false;
		}
	}
}