using System;
using System.Text;

public class Encryption
{
    private string key;
    public Encryption()
    {
        key = "0110";
    }

    public string EncryptDecrypt(String text)
    {
        var result = new StringBuilder();

        for (int c = 0; c < text.Length; c++)
        {
            // take next character from string
            char character = text[c];

            // cast to a uint
            uint charCode = character;

            // figure out which character to take from the key
            int keyPosition = c % key.Length;

            // take the key character
            char keyChar = key[keyPosition];

            // cast it to a uint also
            uint keyCode = keyChar;

            // perform XOR on the two character codes
            uint combinedCode = charCode ^ keyCode;

            // cast back to a char
            char combinedChar = (char)combinedCode;

            // add to the result
            result.Append(combinedChar);
        }

        return result.ToString();
    }
}

