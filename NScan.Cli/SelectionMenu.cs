using System.Text;

namespace NScan.Cli;

public class SelectionMenu(string banner, string[] options)
{
    private readonly string _banner = banner;
    private readonly string[] _options = options;

    public int ShowMenu()
    {
        Clear();
        OutputEncoding = Encoding.UTF8;
        CursorVisible = false;
        ForegroundColor = ConsoleColor.White;
        WriteLine(_banner);
        ResetColor();
        WriteLine("\nUse ↑ and ↓ to navigate and press Enter/Return to select a scan method\n");
        (int left, int top) = GetCursorPosition();
        var option = 0;
        var decorator = ">> ";
        ConsoleKeyInfo key;
        bool isSelected = false;

        while (!isSelected)
        {
            SetCursorPosition(left, top);

            for (int i = 0; i < _options.Length; i++)
            {
                string output = $"{(option == i ? decorator : "   ")}{_options[i]}";
                ForegroundColor = option == i ? ConsoleColor.Green : ConsoleColor.White;
                WriteLine(output);
                ResetColor();
            }

            key = ReadKey(false);

            int optionsCount = _options.Length - 1;
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    option = option == 0 ? optionsCount : option - 1;
                    break;

                case ConsoleKey.DownArrow:
                    option = option == optionsCount ? 0 : option + 1;
                    break;

                case ConsoleKey.Enter:
                    isSelected = true;
                    break;
            }
        }

        CursorVisible = true;
        return option;
    }
}