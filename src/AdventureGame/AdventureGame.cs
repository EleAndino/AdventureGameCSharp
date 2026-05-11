using System.Collections;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace AdventureGame;

public class AdventureGame
{
	public readonly string GO_NORTH = "W";
	public readonly string GO_SOUTH = "S";
	public readonly string GO_EAST = "D";
	public readonly string GO_WEST = "A";
	public readonly string GET_LAMP = "L";
	public readonly string GET_KEY = "K";
	public readonly string OPEN_CHEST = "O";
	public readonly string QUIT = "Q";
    private const char Wall = '#';

    private Adventurer adventurer;
	private Room[,] dungeon;
	private int aRow;
	private int aCol;
    private int exitRow;
    private int exitCol;
    private int lampRow;
    private int lampCol;
    private int keyRow;
    private int keyCol;
    private int chestRow;
    private int chestCol;
    private int grueRow;
    private int grueCol;
    private bool isChestOpen;
	private bool hasPlayerQuit;
	private bool hasAdventurerDied;
	private bool hasAdventurerExitedDungeon;
	private string lastDirection;

	public AdventureGame()
	{

	}

	public void Start()
	{
		Init();

		ShowGameStartScreen();

		string input;

		do
		{
			ShowScene();

			do
			{
				ShowInputOptions();

				input = GetInput();
			}
			while(!IsValidInput(input));

			ProcessInput(input);

			UpdateGameState();
		}
		while(!IsGameOver());

		ShowGameOverScreen();
	}

	private void Init()
	{
		adventurer = new Adventurer();

        dungeon = Load("DungeonLayout.txt");

		aRow = 0;
		aCol = 1;

		isChestOpen = false;
		hasPlayerQuit = false;
		hasAdventurerDied = false;
		hasAdventurerExitedDungeon = false;

		lastDirection = string.Empty;
	}

	private void ShowGameStartScreen()
	{
		Console.WriteLine("Welcome to Adventure Game!");
	}

	private void ShowScene()
	{
		var r = dungeon[aRow, aCol];

		if(adventurer.HasLamp() || r.IsLit())
		{
			Console.WriteLine(r.GetDescription());
		}
		else
		{
			Console.WriteLine("This room is pitch black!");
		}
	}

	private void ShowInputOptions()
	{
		string options = ""
		+ $"GO NORTH [{GO_NORTH}] | GO EAST [{GO_EAST}] | GET LAMP [{GET_LAMP}] | OPEN CHEST [{OPEN_CHEST}]\n"
		+ $"GO SOUTH [{GO_SOUTH}] | GO WEST [{GO_WEST}] | GET KEY  [{GET_KEY}] | QUIT       [{QUIT}]\n"
		+ $"> ";

		Console.Write(options);
	}

	private string GetInput()
	{
		return Console.ReadLine()!.ToUpper();
	}

	private bool IsValidInput(string input)
	{
		string[] validInputs = { GO_NORTH, GO_SOUTH, GO_EAST, GO_WEST, GET_LAMP, GET_KEY, OPEN_CHEST, QUIT };

		if(!validInputs.Contains(input))
		{
			Console.WriteLine("ERROR: Invalid input. Please try again.");
			return false;
		}

		return true;
	}

	private void ProcessInput(string input)
	{
		Room r = dungeon[aRow, aCol];

		if(!adventurer.HasLamp() && !r.IsLit() && input != lastDirection)
		{
			hasAdventurerDied = true;
		}
		else if(input == GO_NORTH)
		{
			GoNorth(r);
		}
		else if(input == GO_SOUTH)
		{
			GoSouth(r);
		}
		else if(input == GO_EAST)
		{
			GoEast(r);
		}
		else if(input == GO_WEST)
		{
			GoWest(r);
		}
		else if(input == GET_LAMP)
		{
			GetLamp(r);
		}
		else if(input == GET_KEY)
		{
			GetKey(r);
		}
		else if(input == OPEN_CHEST)
		{
			OpenChest(r);
		}
		else// if(input == QUIT)
		{
			Quit();
		}
	}

	private void UpdateGameState()
	{
		if(isChestOpen)
		{
			List<(int row, int col)> path = FindPathToAdventurer();

			Console.Write(string.Join(" ", path));

			grueRow = path[1].row;
			grueCol = path[1].col;

			hasAdventurerDied = (grueRow == aRow && grueCol == aCol);
			hasAdventurerExitedDungeon = (exitRow == aRow && exitCol == aCol);
		}
	}

    private bool IsGameOver()
	{
		return hasAdventurerExitedDungeon || hasPlayerQuit || hasAdventurerDied;
	}

	private void ShowGameOverScreen()
	{
		ShowScene();
		Console.WriteLine("Game Over!");
		if (hasPlayerQuit)
		{
			Console.WriteLine("You quit the game.");
		}
		else if (hasAdventurerDied)
		{
            Console.WriteLine("You lost! You have been eaten alive by the Grue!");
        }
		else if (hasAdventurerExitedDungeon)
		{
			Console.WriteLine("Congratulations! You succesfully exited the dungeon with the treasure!");
		}

	}

	private void GoNorth(Room r)
	{
		if(r.HasNorth())
		{
			aRow -= 1;
			lastDirection = GO_SOUTH;
		}
		else
		{
			Console.WriteLine("You cannot go north!\a");
		}
	}

	private void GoSouth(Room r)
	{
		if(r.HasSouth())
		{
			aRow += 1;
			lastDirection = GO_NORTH;
		}
		else
		{
			Console.WriteLine("You cannot go south!\a");
		}
	}

	private void GoEast(Room r)
	{
		if(r.HasEast())
		{
			aCol += 1;
			lastDirection = GO_WEST;
		}
		else
		{
			Console.WriteLine("You cannot go east!\a");
		}
	}

	private void GoWest(Room r)
	{
		if(r.HasWest())
		{
			aCol -= 1;
			lastDirection = GO_EAST;
		}
		else
		{
			Console.WriteLine("You cannot go west!\a");
		}
	}

	private void GetLamp(Room r)
	{
		if(r.HasLamp())
		{
			Console.WriteLine("You got the lamp!");
			adventurer.SetLamp(true);
			r.SetLamp(false);
		}
		else
		{
			Console.WriteLine("There is no lamp in this room.");
		}
	}

	private void GetKey(Room r)
	{
		if(r.HasKey())
		{
			Console.WriteLine("You got the key!");
			adventurer.SetKey(true);
			r.SetKey(false);
		}
		else
		{
			Console.WriteLine("There is no key in this room.");
		}
	}

	private void OpenChest(Room r)
	{
		if(r.HasChest())
		{
			if(adventurer.HasKey())
			{
				Console.WriteLine("You got the treasure!");
				isChestOpen = true;
			}
			else
			{
				Console.WriteLine("You do not have the key!");
			}
		}
		else
		{
			Console.WriteLine("There is no chest in this room.");
		}
	}

	private void Quit()
	{
		hasPlayerQuit = true;
	}

	private List<(int, int)> GetAdjacents(int row, int col)
	{
		var adjs = new List<(int, int)>();

		Room r = dungeon[row, col];

		if(r.HasNorth()) { adjs.Add((row - 1, col - 0)); }
        if (r.HasSouth()) { adjs.Add((row + 1, col + 0)); }
        if (r.HasWest()) { adjs.Add((row - 0, col - 1)); }
        if (r.HasEast()) { adjs.Add((row + 0, col + 1)); }

		return adjs;
    }

    private List<(int row, int col)> FindPathToAdventurer()
    {
		var start = (row: grueRow, col: grueCol);
		var goal = (row: aRow, col: aCol);

        Hashtable path = new Hashtable();

        Hashtable gCost = new Hashtable();

        Hashtable hCost = new Hashtable();

        PriorityQueue<(int row, int col), double> open = new PriorityQueue<(int row, int col), double>();

        path.Add(start, null);

        gCost.Add(start, 0.0);

        hCost.Add(start, 0.0 + GetHeuristic(start, goal));

        open.Enqueue(start, 0.0);

        while (open.Count != 0 && open is not null)
        {

            (int row, int col) n = ((int row, int col))open.Dequeue();

            if (n.row == goal.row && n.col == goal.col)
            {
				List<(int row, int col)> p = path.Keys.OfType<(int row, int col)>().ToList();
                return p;
            }
            else
            {
                foreach ((int row, int col) a in GetAdjacents(n.row, n.col))
                {
                    double oldCost = (double)gCost[a];

                    double newCost = (double)gCost[n] + 1;

                    if (oldCost == null || newCost < oldCost)
                    {
                        path.Add(a, n);

                        gCost.Add(a, newCost);

                        hCost.Add(a, newCost + GetHeuristic(a, goal));
						
                        open.Remove(a);

                        open.Enqueue(a, newCost);
                    }
                }
            }
        }

        return null;
    }

	private static int GetHeuristic((int row, int col) a, (int row, int col) b)
	{
		return Math.Abs((a.row - b.row)) + Math.Abs((a.col - b.col));
	}

	private static List<(int row, int col)> ReconstructPath(Dictionary<(int row, int col), (int row, int col)> cameFrom, (int row, int col) current)
	{
		var path = new List<(int row, int col)> { current };
		while (cameFrom.ContainsKey(current))
		{
			current = cameFrom[current];
			path.Add(current);
		}

		path.Reverse();
		return path;
	}

    public Room[,] Load(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        int rows = int.Parse(lines[0]);
        int cols = int.Parse(lines[1]);

        exitRow = int.Parse(lines[2]);
        exitCol = int.Parse(lines[3]);
        lampRow = int.Parse(lines[4]);
        lampCol = int.Parse(lines[5]);
        keyRow = int.Parse(lines[6]);
        keyCol = int.Parse(lines[7]);
        chestRow = int.Parse(lines[8]);
        chestCol = int.Parse(lines[9]);
        grueRow = int.Parse(lines[10]);
        grueCol = int.Parse(lines[11]);

        int layoutStart = 12;
        int descriptionsStart = layoutStart + rows;

        if (lines.Length < descriptionsStart)
            throw new FormatException("File does not contain enough layout rows.");

        Room[,] dungeon = new Room[rows, cols];
        List<(int row, int col)> traversableTiles = new();

        for (int row = 0; row < rows; row++)
        {
            string layoutLine = lines[layoutStart + row];

            if (layoutLine.Length != cols)
                throw new FormatException($"Layout row {row} must contain exactly {cols} characters.");

            for (int col = 0; col < cols; col++)
            {
                if (layoutLine[col] != Wall)
                {
                    dungeon[row, col] = new Room();
                    traversableTiles.Add((row, col));
                }
            }
        }

        int descriptionCount = lines.Length - descriptionsStart;

        if (descriptionCount != traversableTiles.Count)
        {
            throw new FormatException(
                    $"Description count ({descriptionCount}) must match traversable tile count ({traversableTiles.Count})."
            );
        }

        for (int i = 0; i < traversableTiles.Count; i++)
        {
            string[] parts = lines[descriptionsStart + i].Split('|', 2);

            if (parts.Length != 2)
                throw new FormatException($"Invalid room description line: {lines[descriptionsStart + i]}");

            bool isLit = parts[0] switch
            {
                "1" => true,
                "0" => false,
                _ => throw new FormatException("Room lit value must be 1 or 0.")
            };

            string description = parts[1];

            var (row, col) = traversableTiles[i];
            Room room = dungeon[row, col];

            room.SetLit(isLit);
            room.SetDescription(description);

            room.SetLamp(row == lampRow && col == lampCol);
            room.SetKey(row == keyRow && col == keyCol);
            room.SetChest(row == chestRow && col == chestCol);

            room.SetNorth(IsTraversable(dungeon, row - 1, col));
            room.SetSouth(IsTraversable(dungeon, row + 1, col));
            room.SetEast(IsTraversable(dungeon, row, col + 1));
            room.SetWest(IsTraversable(dungeon, row, col - 1));
        }

        ValidateTraversableTile(dungeon, exitRow, exitCol, "exit");

        return dungeon;
    }

    private bool IsTraversable(Room[,] dungeon, int row, int col)
    {
        return row >= 0 &&
                     row < dungeon.GetLength(0) &&
                     col >= 0 &&
                     col < dungeon.GetLength(1) &&
                     dungeon[row, col] != null;
    }

    private void ValidateTraversableTile(Room[,] dungeon, int row, int col, string name)
    {
        if (!IsTraversable(dungeon, row, col))
            throw new FormatException($"The {name} position must be on a traversable tile.");
    }
}
