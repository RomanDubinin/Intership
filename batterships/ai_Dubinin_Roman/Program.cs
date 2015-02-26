using System;
using System.Linq;
using battleships;

namespace ai_Dubinin_Roman
{
	class Program
	{
		// line имеет один из следующих форматов:
		// Init <map_width> <map_height> <ship1_size> <ship2_size> ...
		// Wound <last_shot_X> <last_shot_Y>
		// Kill <last_shot_X> <last_shot_Y>
		// Miss <last_shot_X> <last_shot_Y>
		// Один экземпляр вашей программы может быть использван для проведения нескольких игр подряд.
		// Сообщение Init сигнализирует о том, что началась новая игра.

		static void Main()
		{
			var gameData = GetGameData();
			var gamer = InitGamer(gameData);
			Vector lastCell;
			Vector nextCell;

			nextCell = gamer.GetNextCell();
			DoShot(nextCell.X, nextCell.Y);

			gameData = GetGameData();
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
					lastCell = ShotCell(gameData);
					nextCell = gamer.GetNextCell(lastCell, lastEffect);
				}
				DoShot(nextCell.X, nextCell.Y);

				gameData = GetGameData();
			}
		}

		private static bool IsInitString(string gameData)
		{
			return gameData.Split(' ').First().CompareTo("Init") == 0;
		}

		private static Vector ShotCell(string gameData)
		{
			var shotParams = gameData.Split(' ');
			var width = int.Parse(shotParams[1]);
			var height = int.Parse(shotParams[2]);
			return new Vector(width, height);
		}

		private static BattleshipGamer InitGamer(string gameData)
		{
			var gameParams = gameData.Split(' ');
			var width = int.Parse(gameParams[1]);
			var height = int.Parse(gameParams[2]);
			return new BattleshipGamer(width, height);
		}

		private static string GetGameData()
		{
			return Console.ReadLine();
		}

		private static void DoShot(int x, int y)
		{
			Console.WriteLine("{0} {1}", x, y);
		}

		private static ShotEffect ShotResult(string data)
		{
			var effestName = data.Split(' ').First();
			return (ShotEffect)Enum.Parse(typeof(ShotEffect), effestName);
		}

		
	}
}
