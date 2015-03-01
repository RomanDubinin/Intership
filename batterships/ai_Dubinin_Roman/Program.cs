using System;
using System.Linq;
using battleships;

namespace ai_Dubinin_Roman
{
	class Program
	{
		static void Main()
		{
			var gameData = GameData();
			var gamer = InitGamer(gameData);

			var nextCell = gamer.GetNextCell();
			DoShot(nextCell.X, nextCell.Y);

			gameData = GameData();
			while (gameData != null)
			{
				
				if (IsInitString(gameData))
				{
					gamer = InitGamer(gameData);
					nextCell = gamer.GetNextCell();
				}
				else
				{
					var lastEffect = ShotResult(gameData);
					var lastCell = GetCell(gameData);
					nextCell = gamer.GetNextCell(lastCell, lastEffect);
				}
				DoShot(nextCell.X, nextCell.Y);

				gameData = GameData();
			}
		}

		private static bool IsInitString(string gameData)
		{
			return String.Compare(gameData.Split(' ').First(), "Init", StringComparison.Ordinal) == 0;
		}

		private static Vector GetCell(string lastShotData)
		{
			var shotParams = lastShotData.Split(' ');
			var width = int.Parse(shotParams[1]);
			var height = int.Parse(shotParams[2]);
			return new Vector(width, height);
		}

		private static BattleshipGamer InitGamer(string lastShotData)
		{
			var gameParams = lastShotData.Split(' ');
			var width = int.Parse(gameParams[1]);
			var height = int.Parse(gameParams[2]);
			return new BattleshipGamer(width, height);
		}

		private static string GameData()
		{
			return Console.ReadLine();
		}

		private static void DoShot(int x, int y)
		{
			Console.WriteLine("{0} {1}", x, y);
		}

		private static ShotEffect ShotResult(string lastShotData)
		{
			var effestName = lastShotData.Split(' ').First();
			return (ShotEffect)Enum.Parse(typeof(ShotEffect), effestName);
		}

		
	}
}
