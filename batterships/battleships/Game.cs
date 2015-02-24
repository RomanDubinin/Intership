using System;
using System.Linq;
using NLog;

namespace battleships
{
	public class ShotInfo
	{
		public ShotEffect Hit;
		public Vector Target;
	}

	public class Game
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		private readonly Ai ai;

		public Game(Map map, Ai ai)
		{
			Map = map;
			this.ai = ai;
			TurnsCount = 0;
			BadShots = 0;
		}

		public Vector LastTarget { get; private set; }
		public int TurnsCount { get; private set; }
		//<summary>Количество сделанных "глупых" ходов</summary>
		public int BadShots { get; private set; }
		public Map Map { get; private set; }
		public ShotInfo LastShotInfo { get; private set; }
		public bool AiCrashed { get; private set; }
		public Exception LastError { get; private set; }

		public bool IsOver()
		{
			return !Map.HasAliveShips() || AiCrashed;
		}

		public void MakeStep()
		{
			if (IsOver()) throw new InvalidOperationException("Game is Over");
			if (!UpdateLastTarget()) return;
			if (IsBadShot(LastTarget)) BadShots++;
			var hit = Map.Badaboom(LastTarget);
			LastShotInfo = new ShotInfo {Target = LastTarget, Hit = hit};
			if (hit == ShotEffect.Miss)
				TurnsCount++;
		}

		private bool UpdateLastTarget()
		{
			try
			{
				LastTarget = LastTarget == null
					? ai.Init(Map.MapWidth, Map.MapHeight, Map.Ships.Select(s => s.ShipSize).ToArray())
					: ai.GetNextShot(LastShotInfo.Target, LastShotInfo.Hit);
				return true;
			}
			catch (Exception e)
			{
				AiCrashed = true;
				log.Info("Ai {0} crashed", ai.Name);
				log.Error(e);
				LastError = e;
				return false;
			}
		}

		private bool IsBadShot(Vector target)
		{
			var cellWasHitAlready = Map[target] != CellState.Empty && Map[target] != CellState.Ship;
			var cellIsNearDestroyedShip = Map.Neighbours(target).Any(c => Map.ShipsMap[c.X, c.Y] != null && !Map.ShipsMap[c.X, c.Y].IsAlive);
			var diagonals = new[] { new Vector(-1, -1), new Vector(-1, 1), new Vector(1, -1), new Vector(1, 1) };
			var cellHaveWoundedDiagonalNeighbour = diagonals
				.Where(d => Map.CheckBounds(target.Add(d)))
				.Any(d => Map[target.Add(d)] == CellState.DeadOrWoundedShip);

			return cellWasHitAlready || cellIsNearDestroyedShip || cellHaveWoundedDiagonalNeighbour;
		}
	}
}