using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Consts;

public class History
{
    public int column;
    public int row;
    public PlayerDisk playerDisk;

    public History(int column, int row, PlayerDisk playerDisk)
    {
        this.column = column;
        this.row = row;
        this.playerDisk = playerDisk;
    }

    public static int CompareByColumn(History his1, History his2)
    {
        if (his1.column > his2.column) return 1;

        return 0;
    }
    public static List<List<History>> DivideBoardToRows(List<History> history, int maxRows)
    {
        List<List<History>> dividedBoard = new();
        for (int i = 0; i < maxRows; i++)
        {
            List<History> row = history.FindAll(historyItem => historyItem.row == i);
            row.Sort((e, e1) => e.column.CompareTo(e1.column));
            dividedBoard.Add(row);
        }
        return dividedBoard;

    }
    public static List<List<History>> DivideBoardToColumns(List<History> history, int maxCol)
    {
        List<List<History>> dividedBoard = new();

        for (int i = 0; i < maxCol; i++)
        {
            List<History> column = history.FindAll(historyItem => historyItem.column == i);
            dividedBoard.Add(column);
        }

        return dividedBoard;
    }
    public static string GenerateMetaString(List<History> boardPart, int maxVal, Choice choice)
    {
        string result = "";

        int lastVal = 0;
        int topVal = 0;


        for (int i = 0; i < boardPart.Count; i++)
        {
            int currentVal = choice == Choice.Column ? boardPart[i].row : boardPart[i].column;

            if (i == 0) { for (int j = 0; j < currentVal; j++) { result += ", "; } }

            result += (boardPart[i].playerDisk);
            lastVal = choice == Choice.Column ? boardPart[i].row : boardPart[i].column;
            if (boardPart.Count > i + 1)
            {
                int nextVal = choice == Choice.Column ? boardPart[i + 1].row : boardPart[i + 1].column;
                if (currentVal < nextVal)
                {
                    for (int j = 0; j < nextVal - currentVal; j++) { result += ", "; }
                }
            }

            if (lastVal > topVal) topVal = lastVal;
        }

        if (topVal < maxVal)
            for (int i = 0; i < maxVal - lastVal; i++) result += ", ";

        return result;
    }
    public static void DisplayBoard(List<History> board, int maxCol, int maxRows, bool col, bool row)
    {
        string result = "";
        if (col)
        {
            Debug.Log("Columns\n");
            List<List<History>> dividedBoardColumns = DivideBoardToColumns(board, maxCol);

            dividedBoardColumns.ForEach(col =>
            {
                result = GenerateMetaString(col, maxRows, Choice.Column);
                Debug.Log(result);
            });
        }
        if (row)
        {
            Debug.Log("Rows\n");
            List<List<History>> dividedBoardRows = DivideBoardToRows(board, maxRows);

            dividedBoardRows.ForEach(row =>
            {
                result = GenerateMetaString(row, maxCol, Choice.Row);
                Debug.Log(result);
            });
        }
    }

    public static bool CheckWin(List<History> board, int maxCol, int maxRows, string currentDisk)
    {
        string result = "";
        List<List<History>> dividedBoardColumns = DivideBoardToColumns(board, maxCol);
        bool win = false;
        // horizontal
        dividedBoardColumns.ForEach(column =>
        {
            result = GenerateMetaString(column, maxRows, Choice.Column);
            if (result.Contains($"{currentDisk}, {currentDisk}, {currentDisk}, {currentDisk}, "))
            {
                win = true;
            }
        });
        if (win) return true;

        //vertical
        List<string> boardString = new();
        List<List<History>> dividedBoardRows = DivideBoardToRows(board, maxRows);
        dividedBoardRows.ForEach(row =>
        {
            result = GenerateMetaString(row, maxRows, Choice.Row);
            boardString.Add(result);

            if (result.Contains($"{currentDisk}, {currentDisk}, {currentDisk}, {currentDisk}, ")
            || result.Contains($"{currentDisk}, {currentDisk}, {currentDisk}, {currentDisk}"))
            {
                win = true;
            }
        });
        if (win) return true;

        //diagonal
        int[,] rowsA = new int[maxCol, maxRows];

        for (int i = 0; i < maxCol; i++)
        {
            for (int j = 0; j < maxRows; j++)
            {
                rowsA[i, j] = 0;
            }
        }

        DivideBoardToRows(board, maxRows).ForEach(row =>
        {
            row.ForEach(item =>
            {
                rowsA[item.column, item.row] = item.playerDisk == PlayerDisk.diskA ? 1 : 2;
            });
        });

        for (int i = 3; i < maxCol; i++)
        {
            for (int j = 0; j < maxRows - 3; j++)
            {
                int currentPlayer = rowsA[i, j];
                if (currentPlayer != 0 && currentPlayer == rowsA[i - 1, j + 1] && currentPlayer == rowsA[i - 2, j + 2] && currentPlayer == rowsA[i - 3, j + 3])
                {
                    win = true;
                }
            }
        }

        for (int i = 3; i < maxCol; i++)
        {
            for (int j = 3; j < maxRows; j++)
            {
                int currentPlayer = rowsA[i, j];
                if (currentPlayer != 0 && currentPlayer == rowsA[i - 1, j - 1] && currentPlayer == rowsA[i - 2, j - 2] && currentPlayer == rowsA[i - 3, j - 3])
                {
                    win = true;
                }
            }
        }

        return win;
    }
}