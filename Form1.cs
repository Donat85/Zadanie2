using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;

namespace Zadanie1
{

    public partial class Form1 : Form
    {

        static class Globals
        {
            public static String instructions = null;
            public static bool complied = true;
            public static int currentline = 1;
            public static bool anyinterrupt = false;
            public static bool interrupt = false;
            public static bool interrupt21h = false;
            public static bool interrupt16h = false;
            public class Stack { };
            public static Stack<String> registerstack = new Stack<string>();
            public static bool zeroflag = false;

        }

        public Form1()
        {
            // Console.WriteLine("siema");

            InitializeComponent();

        }



        private void btnWykonaj_Click(object sender, EventArgs e)
        {

            Globals.instructions = textBox1.Text;
            Globals.interrupt = false;


            checkText(Globals.instructions);
            if (Globals.complied)
            {
                if (Globals.instructions != textBox1.Text)
                {

                    Globals.instructions = textBox1.Text;
                    checkText(Globals.instructions);
                    if (Globals.currentline <= Globals.instructions.Split('\n').Length)
                    {
                        string message = "You have changed the code during step execution, do you wish to start execution from line 1?";
                        string caption = "Detected change of code";
                        MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                        DialogResult result;
                        result = MessageBox.Show(message, caption, buttons);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                            Globals.currentline = 1;
                        }
                    }
                    else
                    {
                        string message = "You have changed the code druing step execution, the new code is shorter than the prievous one. The execution will proceed from the first line";
                        string caption = "Detected change of code";
                        MessageBoxButtons buttons = MessageBoxButtons.OK;
                        DialogResult result;
                        result = MessageBox.Show(message, caption, buttons);
                        Globals.currentline = 1;
                    }

                }

                if (!checkBox1.Checked)
                {

                    String[] instruction = Globals.instructions.Split('\n');

                    for (int i = Globals.currentline - 1; i < instruction.Length; i++)
                    {
                        interpretate(instruction[i]);
                        if (Globals.interrupt)
                        {
                            Globals.currentline = 1;
                            break;
                        }
                        Globals.currentline++;
                        if (i == instruction.Length - 1)
                        {
                            Globals.currentline = 1;
                            Globals.instructions = null;

                        }
                    }

                }
                if (checkBox1.Checked && !Globals.anyinterrupt)
                {
                    String[] instruction = Globals.instructions.Split('\n');
                    if (Globals.currentline < instruction.Length)
                    {
                        interpretate(instruction[Globals.currentline - 1]);
                        if (!Globals.interrupt)
                        {
                            Globals.currentline++;
                        }
                    }
                    else
                    {
                        interpretate(instruction[Globals.currentline - 1]);
                        if (!Globals.interrupt)
                        {
                            Globals.currentline++;
                        }
                        Globals.currentline = 1;
                        Globals.instructions = null;
                        string message = "You have executed all of your instructions, next press of button 'wykonaj' will perform instructions from line one";
                        string caption = "Last step";
                        MessageBoxButtons buttons = MessageBoxButtons.OK;
                        DialogResult result;
                        result = MessageBox.Show(message, caption, buttons);

                    }

                }
            }
            lbl_Linia.Text = Globals.currentline.ToString();
        }
        private void addbinary(String tekstlabel1, String tekstlabel2, String registername)
        {


            String result = null;
            int carry = 0;
            bool overflow = false;
            int[] output = new int[8];
            for (int i = 7; i >= 0; i--)
            {
                int bit1 = tekstlabel1[i] - '0';
                int bit2 = tekstlabel2[i] - '0';
                output[i] = bit1 + bit2 + carry;
                if (output[i] > 1)
                {
                    if (output[i] == 3)
                    {
                        output[i] = 1;
                    }
                    else
                    {
                        output[i] = 0;
                    }
                    carry = 1;
                    if (i == 0)
                    {
                        overflow = true;
                    }
                }
                else
                {
                    carry = 0;
                }


            }
            for (int i = 0; i < 8; i++)
            {
                result += output[i].ToString();
            }
            if (overflow)
            {
                string message = "Overflow of the register, its value has been set to 0. \n Yes=continue, No=set current line to 1 and stop executing instructions.";
                string caption = "Overflow";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result2;
                result2 = MessageBox.Show(message, caption, buttons);
                if (result2 == System.Windows.Forms.DialogResult.No)
                {
                    Globals.currentline = 1;
                    Globals.interrupt = true;
                }
            }

            movebinary(registername, result);
        }
        private void subbinary(String tekstlabel1, String tekstlabel2, String registername)
        {




            String result = null;
            int[] output = new int[8];
            bool negative = false;
            int[] bit1 = new int[8];
            int[] bit2 = new int[8];

            int a = Convert.ToInt32(tekstlabel1, 2);
            int b = Convert.ToInt32(tekstlabel2, 2);
            if (a >= b)
            {
                for (int i = 0; i < 8; i++)
                {
                    bit1[i] = tekstlabel1[i] - '0';
                    bit2[i] = tekstlabel2[i] - '0';
                }
                for (int i = 7; i >= 0; i--)
                {


                    output[i] = bit1[i] - bit2[i];
                    if (output[i] < 0)
                    {
                        int back = carryfinder(i, bit1, 1);
                        output[i] = 1;
                        bit1[i - back] = 0;
                        for (int j = back - 1; j > 0; j--)
                        {
                            bit1[i - j] = 1;
                        }

                    }


                }
            }
            else
            {
                negative = true;
                for (int i = 0; i < 8; i++)
                {
                    bit2[i] = tekstlabel1[i] - '0';
                    bit1[i] = tekstlabel2[i] - '0';
                }
                for (int i = 7; i >= 0; i--)
                {


                    output[i] = bit1[i] - bit2[i];
                    if (output[i] < 0)
                    {
                        int back = carryfinder(i, bit1, 1);
                        output[i] = 1;
                        bit1[i - back] = 0;
                        for (int j = back - 1; j > 0; j--)
                        {
                            bit1[i - j] = 1;
                        }

                    }


                }
            }
            for (int i = 0; i < 8; i++)
            {
                result += output[i];
            }
            movebinary(registername, result);
        }
        private int carryfinder(int index, int[] bin, int back)
        {

            if (bin[index - 1] == 1)
            {
                return back;
            }
            else
            {
                back++;
                back = carryfinder(index - 1, bin, back);
                return back;
            }

        }
        private void movebinary(String registername, String value)
        {
            switch (registername)
            {
                case "AH":
                    lblAH.Text = value;
                    break;
                case "AL":
                    lblAL.Text = value;
                    break;
                case "BH":
                    lblBH.Text = value;
                    break;
                case "BL":
                    lblBL.Text = value;
                    break;
                case "CH":
                    lblCH.Text = value;
                    break;
                case "CL":
                    lblCL.Text = value;
                    break;
                case "DH":
                    lblDH.Text = value;
                    break;
                case "DL":
                    lblDL.Text = value;
                    break;
                case "AX":
                    lblAH.Text = value.Substring(0, 8);
                    lblAL.Text = value.Substring(8, 8);
                    break;
                case "BX":
                    lblBH.Text = value.Substring(0, 8);
                    lblBL.Text = value.Substring(8, 8);
                    break;
                case "CX":
                    lblCH.Text = value.Substring(0, 8);
                    lblCL.Text = value.Substring(8, 8);
                    break;
                case "DX":
                    lblDH.Text = value.Substring(0, 8);
                    lblDL.Text = value.Substring(8, 8);
                    break;



            }

        }
        private void btnWczytaj_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
            try
            {
                string path = this.openFileDialog1.FileName;
                string temp = null;
                for (int i = 0; i < 4; i++)
                {
                    temp += path[path.Length - 4 + i];
                }
                if (temp == ".txt")
                    textBox1.Text = System.IO.File.ReadAllText(@path);
                else
                    MessageBox.Show("Invalid file.", "Error", MessageBoxButtons.OK);
            }
            catch
            {
                MessageBox.Show("Error loading from file.", "Error", MessageBoxButtons.OK);
            }

        }
        private void btnZapisz_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
            try
            {
                string path = this.openFileDialog1.FileName;
                string temp = null;
                for (int i = 0; i < 4; i++)
                {
                    temp = temp + path[path.Length - 4 + i];
                }
                if (temp == ".txt")
                    System.IO.File.WriteAllText(@path, textBox1.Text);
                else
                    MessageBox.Show("Invalid file.", "Error", MessageBoxButtons.OK);
            }
            catch
            {
                MessageBox.Show("Error writing to file.", "Error", MessageBoxButtons.OK);
            }
        }
        private int interpretate(String line)
        {
            String[] words = line.Split(' ', '\r');
            if (words[0].Substring(0, 3) == "INT")
            {
                interruptfunctions(words[0]);
                return 0;
            }
            if (words[0] == "PUSH")
            {
                pushstack(words[1]);
                return 0;
            }
            if (words[0] == "POP")
            {
                popstack(words[1]);
                return 0;
            }

            words[2].ToUpper();
            if (words[2][words[2].Length - 1] == 'H' && isHex(words[2].Substring(0, words[2].Length - 1))) {
                hextobin(ref words[2]);
                completenumber(ref words[2], words[1][1]);
            }

            if (words[1] == "AX" || words[1] == "BX" || words[1] == "CX" || words[1] == "DX")
            {
                divideRegister(words);

                return 0;
            }
            if (words[2] == "AX" || words[2] == "BX" || words[2] == "CX" || words[2] == "DX")
            {
                Globals.interrupt = true;
                string message = "Current line is trying to bring 16bit value under 8 bit value";
                string caption = "Wrong action";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;
                result = MessageBox.Show(message, caption, buttons);
                return 1;
            }

            String arg1 = registergetvalue(words[1]);
            String arg2 = registergetvalue(words[2]);

            switch (words[0]) {
                case "ADD": addbinary(arg1, arg2, words[1]);
                    break;
                case "SUB":
                    subbinary(arg1, arg2, words[1]);
                    break;
                case "MOV":
                    movebinary(words[1], arg2);
                    break;

            }
            return 0;
        }
        private String registergetvalue(String registername)
        {
            String result = null;
            switch (registername)
            {
                case "AH":
                    result = lblAH.Text;
                    break;
                case "AL":
                    result = lblAL.Text;
                    break;
                case "BH":
                    result = lblBH.Text;
                    break;
                case "BL":
                    result = lblBL.Text;
                    break;
                case "CH":
                    result = lblCH.Text;
                    break;
                case "CL":
                    result = lblCL.Text;
                    break;
                case "DH":
                    result = lblDH.Text;
                    break;
                case "DL":
                    result = lblDL.Text;
                    break;
                case "AX":
                    result = lblAH.Text + lblAL.Text;
                    break;
                case "BX":
                    result = lblBH.Text + lblBL.Text;
                    break;
                case "CX":
                    result = lblCH.Text + lblCL.Text;
                    break;
                case "DX":
                    result = lblDH.Text + lblDL.Text;
                    break;
                default:
                    result = registername;
                    break;
            }
            return result;

        }
        private int checkText(String text)
        {
            Globals.complied = true;
            String[] lines = text.Split('\n');



            for (int i = 0; i < lines.Length; i++)
            {
                Console.WriteLine(i);
                String[] words = null;
                words = lines[i].Split(' ', '\r');
                Console.WriteLine(firstword(words));

                if (words.Length > 4)
                {


                    Globals.complied = false;
                    break;
                }
                else
                {
                    if (words.Length == 4)
                    {
                        if (words[3] != "\r" && words[3] != "")
                        {
                            Globals.complied = false;
                            break;
                        }
                    }


                    if (!firstword(words))
                    {
                        Globals.complied = false;
                        break;
                    }

                }

            }
            if (!Globals.complied)
            {
                string message = "There is a mistake in your code";
                string caption = "Instructions error";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;
                result = MessageBox.Show(message, caption, buttons);
            }
            return 0;
        }
        private bool firstword(String[] words)
        {
            words[0].ToUpper();

            switch (words[0]) {
                case "ADD":
                    return secondword(words);
                case "SUB":
                    return secondword(words);
                case "MOV":
                    return secondword(words);
                case "POP":
                    return secondword(words);
                case "PUSH":
                    return secondword(words);
                default:
                    if (words[0].Substring(0, 3) == "INT" && (words.Length == 1 || words[1] == "" || words[1] == "\r"))
                    {
                        return true;
                    }
                    return false;
            }

        }
        private bool secondword(String[] words)
        {
            words[1].ToUpper();
            if (words[0] == "POP" || words[0] == "PUSH" && (words.Length == 2 || words[2] == "" || words[2] == "\r"))
            {
                switch (words[1])
                {
                    case "AH":
                        return true;
                    case "AL":
                        return true;
                    case "BH":
                        return true;
                    case "BL":
                        return true;
                    case "CH":
                        return true;
                    case "CL":
                        return true;
                    case "DH":
                        return true;
                    case "DL":
                        return true;
                    case "AX":
                        return true;
                    case "BX":
                        return true;
                    case "CX":
                        return true;
                    case "DX":
                        return true;

                }
            }
            switch (words[1])
            {
                case "AH":
                    return thirdword(words);
                case "AL":
                    return thirdword(words);
                case "BH":
                    return thirdword(words);
                case "BL":
                    return thirdword(words);
                case "CH":
                    return thirdword(words);
                case "CL":
                    return thirdword(words);
                case "DH":
                    return thirdword(words);
                case "DL":
                    return thirdword(words);
                case "AX":
                    return thirdword(words);
                case "BX":
                    return thirdword(words);
                case "CX":
                    return thirdword(words);
                case "DX":
                    return thirdword(words);
                default:
                    return false;
            }
        }
        private bool thirdword(String[] words)
        {
            words[2].ToUpper();
            switch (words[2])
            {
                case "AH":
                    return true;
                case "AL":
                    return true;
                case "BH":
                    return true;
                case "BL":
                    return true;
                case "CH":
                    return true;
                case "CL":
                    return true;
                case "DH":
                    return true;
                case "DL":
                    return true;
                case "AX":
                    return true;
                case "BX":
                    return true;
                case "CX":
                    return true;
                case "DX":
                    return true;
                default:
                    if (words[2].Length == 8 || words[2].Length == 16)
                    {
                        for (int i = 0; i < words[2].Length; i++)
                        {
                            if (words[2][i] != '1' && words[2][i] != '0')
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    if (words[2][words[2].Length - 1] == 'H')
                    {
                        if (isHex(words[2].Substring(0, words[2].Length - 1)))
                        {
                            return true;
                        }
                    }
                    return false;
            }
        }
        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                lbl_AktLinia.Visible = true;
                lbl_Linia.Visible = true;
            }
            else
            {
                lbl_AktLinia.Visible = false;
                lbl_Linia.Visible = false;
            }


        }
        private int divideRegister(String[] words)
        {

            if (words[2] == "AX" || words[2] == "BX" || words[2] == "CX" || words[2] == "DX")
            {
                String[] newline = new string[3];
                newline[0] = words[0];
                switch (words[1])
                {
                    case "AX":
                        switch (words[2])
                        {
                            case "AX":
                                newline[1] = "AH";
                                newline[2] = "AH";
                                interpretate16bit(newline);
                                newline[1] = "AL";
                                newline[2] = "AL";
                                interpretate16bit(newline);
                                break;
                            case "BX":
                                newline[1] = "AH";
                                newline[2] = "BH";
                                interpretate16bit(newline);
                                newline[1] = "AL";
                                newline[2] = "BL";
                                interpretate16bit(newline);
                                break;
                            case "CX":
                                newline[1] = "AH";
                                newline[2] = "CH";
                                interpretate16bit(newline);
                                newline[1] = "AL";
                                newline[2] = "CL";
                                interpretate16bit(newline);
                                break;
                            case "DX":
                                newline[1] = "AH";
                                newline[2] = "DH";
                                interpretate16bit(newline);
                                newline[1] = "AL";
                                newline[2] = "DL";
                                interpretate16bit(newline);
                                break;
                        }
                        break;
                    case "BX":
                        switch (words[2])
                        {
                            case "AX":
                                newline[1] = "BH";
                                newline[2] = "AH";
                                interpretate16bit(newline);
                                newline[1] = "BL";
                                newline[2] = "AL";
                                interpretate16bit(newline);
                                break;
                            case "BX":
                                newline[1] = "BH";
                                newline[2] = "BH";
                                interpretate16bit(newline);
                                newline[1] = "BL";
                                newline[2] = "BL";
                                interpretate16bit(newline);
                                break;
                            case "CX":
                                newline[1] = "BH";
                                newline[2] = "CH";
                                interpretate16bit(newline);
                                newline[1] = "BL";
                                newline[2] = "CL";
                                interpretate16bit(newline);
                                break;
                            case "DX":
                                newline[1] = "BH";
                                newline[2] = "DH";
                                interpretate16bit(newline);
                                newline[1] = "BL";
                                newline[2] = "DL";
                                interpretate16bit(newline);
                                break;
                        }
                        break;
                    case "CX":
                        switch (words[2])
                        {
                            case "AX":
                                newline[1] = "CH";
                                newline[2] = "AH";
                                interpretate16bit(newline);
                                newline[1] = "CL";
                                newline[2] = "AL";
                                interpretate16bit(newline);
                                break;
                            case "BX":
                                newline[1] = "CH";
                                newline[2] = "BH";
                                interpretate16bit(newline);
                                newline[1] = "CL";
                                newline[2] = "BL";
                                interpretate16bit(newline);
                                break;
                            case "CX":
                                newline[1] = "CH";
                                newline[2] = "CH";
                                interpretate16bit(newline);
                                newline[1] = "CL";
                                newline[2] = "CL";
                                interpretate16bit(newline);
                                break;
                            case "DX":
                                newline[1] = "CH";
                                newline[2] = "DH";
                                interpretate16bit(newline);
                                newline[1] = "CL";
                                newline[2] = "DL";
                                interpretate16bit(newline);
                                break;
                        }
                        break;
                    case "DX":
                        switch (words[2])
                        {
                            case "AX":
                                newline[1] = "DH";
                                newline[2] = "AH";
                                interpretate16bit(newline);
                                newline[1] = "DL";
                                newline[2] = "AL";
                                interpretate16bit(newline);
                                break;
                            case "BX":
                                newline[1] = "DH";
                                newline[2] = "BH";
                                interpretate16bit(newline);
                                newline[1] = "DL";
                                newline[2] = "BL";
                                interpretate16bit(newline);
                                break;
                            case "CX":
                                newline[1] = "DH";
                                newline[2] = "CH";
                                interpretate16bit(newline);
                                newline[1] = "DL";
                                newline[2] = "CL";
                                interpretate16bit(newline);
                                break;
                            case "DX":
                                newline[1] = "DH";
                                newline[2] = "DH";
                                interpretate16bit(newline);
                                newline[1] = "DL";
                                newline[2] = "DL";
                                interpretate16bit(newline);
                                break;
                        }
                        break;

                }
                return 0;
            }
            if (words[2].Length == 16) {
                String[] newline = new string[3];
                newline[0] = words[0];


                String firstpart = words[2].Substring(0, words[2].Length / 2);
                String secondpart = words[2].Substring(words[2].Length / 2, words[2].Length / 2);
                switch (words[1])
                {
                    case "AX":
                        newline[1] = "AH";
                        newline[2] = firstpart;
                        interpretate16bit(newline);
                        newline[1] = "AL";
                        newline[2] = secondpart;
                        interpretate16bit(newline);
                        break;
                    case "BX":
                        newline[1] = "BH";
                        newline[2] = firstpart;
                        interpretate16bit(newline);
                        newline[1] = "BL";
                        newline[2] = secondpart;
                        interpretate16bit(newline);
                        break;
                    case "CX":
                        newline[1] = "CH";
                        newline[2] = firstpart;
                        interpretate16bit(newline);
                        newline[1] = "CL";
                        newline[2] = secondpart;
                        interpretate16bit(newline);
                        break;
                    case "DX":
                        newline[1] = "DH";
                        newline[2] = firstpart;
                        interpretate16bit(newline);
                        newline[1] = "DL";
                        newline[2] = secondpart;
                        interpretate16bit(newline);
                        break;
                }
                return 1;
            }
            else if (secondword(words))
            {
                String[] newline = new string[3];
                newline[0] = words[0];
                newline[2] = words[2];
                switch (words[1])
                {
                    case "AX":
                        newline[1] = "AL";
                        break;
                    case "BX":
                        newline[1] = "BL";
                        break;
                    case "CX":
                        newline[1] = "CL";
                        break;
                    case "DX":
                        newline[1] = "DL";
                        break;
                }
                interpretate16bit(newline);
                return 1;
            }
            else
            {
                Globals.interrupt = true;
                return -1;
            }


        }
        private void interpretate16bit(String[] words)
        {
            String arg1 = registergetvalue(words[1]);
            String arg2 = registergetvalue(words[2]);
            switch (words[0])
            {
                case "ADD":
                    addbinary(arg1, arg2, words[1]);
                    break;
                case "SUB":
                    subbinary(arg1, arg2, words[1]);
                    break;
                case "MOV":
                    movebinary(words[1], arg2);
                    break;

            }
        }
        private bool isHex(String word)
        {
            bool hex = true;
            for (int i = 0; i < word.Length; i++)
            {
                if ((word[i] < '0' || word[i] > '9') && (word[i] < 'A' || word[i] > 'F'))
                {

                    hex = false;
                }
                if (!hex)
                {
                    return hex;
                }
            }
            return hex;
        }
        private void hextobin(ref String word) {
            word = word.Substring(0, word.Length - 1);
            int decimalnumber = Convert.ToInt32(word, 16);
            word = Convert.ToString(decimalnumber, 2);

        }
        private void completenumber(ref String word, char X)
        {
            String complete = null;
            if (X == 'X') {
                for (int i = 0; i < 16 - word.Length; i++)
                {
                    complete += '0';
                }
                word = complete + word;
            }
            else
            {
                for (int i = 0; i < 8 - word.Length; i++)
                {
                    complete += '0';
                }
                word = complete + word;
            }
        }
        private void pushstack(String register)
        {
            switch (register)
            {
                case "AX":
                    Globals.registerstack.Push(registergetvalue("AH") + registergetvalue("AL"));
                    break;
                case "BX":
                    Globals.registerstack.Push(registergetvalue("BH") + registergetvalue("BL"));
                    break;
                case "CX":
                    Globals.registerstack.Push(registergetvalue("CH") + registergetvalue("CL"));
                    break;
                case "DX":
                    Globals.registerstack.Push(registergetvalue("DH") + registergetvalue("DL"));
                    break;
            }
        }
        private void popstack(String register)
        {
            String buffer = "";
            switch (register)
            {
                case "AX":
                    buffer = Globals.registerstack.Pop();
                    movebinary("AH", buffer.Substring(0, buffer.Length / 2));
                    movebinary("AL", buffer.Substring(buffer.Length / 2, buffer.Length / 2));
                    break;
                case "BX":
                    buffer = Globals.registerstack.Pop();
                    movebinary("BH", buffer.Substring(0, buffer.Length / 2));
                    movebinary("BL", buffer.Substring(buffer.Length / 2, buffer.Length / 2));
                    break;
                case "CX":
                    buffer = Globals.registerstack.Pop();
                    movebinary("CH", buffer.Substring(0, buffer.Length / 2));
                    movebinary("CL", buffer.Substring(buffer.Length / 2, buffer.Length / 2));
                    break;
                case "DX":
                    buffer = Globals.registerstack.Pop();
                    movebinary("DH", buffer.Substring(0, buffer.Length / 2));
                    movebinary("DL", buffer.Substring(buffer.Length / 2, buffer.Length / 2));
                    break;
            }
        }
        private void interruptfunctions(String function)
        {
            function = function.ToUpper();
            switch (function)
            {
                case "INT21H":
                    interruptfunction21h();
                    break;
                case "INT15H":
                    interruptfunction15h();
                    break;
                case "INT16H":
                    interruptfunction16h();
                    break;
                case "INT10H":
                    interruptfunction10h();
                    break;


                    //
            }

        }
        private void interruptfunction21h()
        {
            String argument = registergetvalue("AH");
            int argumentvalue = Convert.ToInt32(argument, 2);
            switch (argumentvalue)
            {
                case 1:
                    Globals.interrupt21h = true;
                    Globals.anyinterrupt = true;
                    Thread.Sleep(2000);
                    break;
                case 2:
                    String value = registergetvalue("DL");
                    int charvalue = Convert.ToInt32(value, 2);
                    Console.WriteLine((char)charvalue);
                    break;
                case 42:
                    currentdate();
                    break;
                case 43:
                    if (checkdate() == 0)
                    {    // przerwanie powinno ustawić datę, w naszym programie nie będziemy ustawiać daty komputera, więc funkcja tylko sprawdzi czy wartości rejestrów odpowiadają istniejącej dacie
                        movebinary("AL", "00000000");
                    }
                    else
                    {
                        movebinary("AL", "11111111");
                    }
                        break;
                case 44:
                    currenttime();
                    break;
                case 45:
                    if (checkdate() == 0)
                    {
                        movebinary("AL", "00000000");
                    }
                    else
                    {
                        movebinary("AL", "11111111");
                    }
                    break;
                case 0:
                    this.Close();
                    break;
            }
            Globals.anyinterrupt = false;
            Globals.interrupt21h = false;
        }
        private void Form1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (Globals.interrupt21h) {
                char character = (char)e.KeyValue;
                String asciinumber= Convert.ToString((int)character, 2);
                completenumber(ref asciinumber, 'l');
                movebinary("AL", asciinumber);

            }
            if (Globals.interrupt16h)
            {
                String output = "0";
                if (Keyboard.IsKeyDown(System.Windows.Input.Key.Insert))
                {
                    output = "1";
                }
                else
                {
                    output = "0";
                }
                if (Keyboard.IsKeyDown(System.Windows.Input.Key.CapsLock))
                {
                    output += "1";
                }
                else
                {
                    output += "0";
                }
                if (Keyboard.IsKeyDown(System.Windows.Input.Key.NumLock))
                {
                    output += "1";
                }
                else
                {
                    output += "0";
                }
                if (Keyboard.IsKeyDown(System.Windows.Input.Key.Scroll))
                {
                    output += "1";
                }
                else
                {
                    output += "0";
                }
                if (Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightAlt))
                {
                    output += "1";
                }
                else
                {
                    output += "0";
                }
                if (Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl))
                {
                    output += "1";
                }
                else
                {
                    output += "0";
                }
                if (Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift))
                {
                    output += "1";
                }
                else
                {
                    output += "0";
                }
                if (Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift))
                {
                    output += "1";
                }
                else
                {
                    output += "0";
                }
                movebinary("AL", output);
                Globals.interrupt16h = false;
            }


        }
        private void currentdate()
        {
            DateTime date = DateTime.Now;
            String year = Convert.ToString(date.Year, 2);
            completenumber(ref year, 'X');
            String month = Convert.ToString(date.Month, 2);
            completenumber(ref month, 'N');
            String day = Convert.ToString(date.Day, 2);
            completenumber(ref day, 'N');
            movebinary("CX", year);
            movebinary("DH", month);
            movebinary("DL", day);
        }
        private int  checkdate()
        {
            int year = Convert.ToInt32(registergetvalue("CX"),2);
            int month = Convert.ToInt32(registergetvalue("DH"),2);
            int day = Convert.ToInt32(registergetvalue("DL"),2);
            if(year<1980 || year > 2099)
            {
                return 1;
            }
            if(month<1 || month > 12)
            {
                return 1;
            }
            if (day<1 || day>31)
            {
                return 1;
            }
            
            return 0;
        }
        private void currenttime()
        {
            DateTime date = DateTime.Now;
            String hour = Convert.ToString(date.Hour, 2);
            completenumber(ref hour, 'N');
            String minute = Convert.ToString(date.Minute, 2);
            completenumber(ref minute, 'N');
            String second = Convert.ToString(date.Second, 2);
            completenumber(ref second , 'N');
            String milisecond = Convert.ToString(date.Millisecond, 2);
            completenumber(ref milisecond, 'N');
            movebinary("CH", hour);
            movebinary("CL", minute);
            movebinary("DH", second);
            movebinary("DL", milisecond);

        }
        private int  checktime()
        {
            int hour = Convert.ToInt32(registergetvalue("CH"),2);
            int minute = Convert.ToInt32(registergetvalue("CL"),2);
            int second = Convert.ToInt32(registergetvalue("DH"),2);
            int milisecond = Convert.ToInt32(registergetvalue("DL"),2);
            if (hour < 0 || hour > 23)
            {
                return 1;
            }
            if (minute < 0 || minute>60)
            {
                return 1;
            }
            if (second < 0 || second > 60)
            {
                return 1;
            }
            if (milisecond < 0 || milisecond > 100)
            {
                return 1;
            }

            return 0;
        }
        private void interruptfunction15h()
        {
            String argument = registergetvalue("AH");
            int argumentvalue = Convert.ToInt32(argument, 2);
            switch (argumentvalue) {
                case 134:
                    int waittime = Convert.ToInt32(registergetvalue("CX"), 2);
                    if (waittime == 0)
                    {
                        waittime = Convert.ToInt32(registergetvalue("DX"), 2);
                    }
                    Thread.Sleep(waittime);
                    Console.WriteLine("Slept for " + waittime + " miliseconds");
                    break;
            }
        }
        private void interruptfunction16h()
        {
            String argument = registergetvalue("AH");
            int argumentvalue = Convert.ToInt32(argument, 2);
            switch (argumentvalue)
            {
                case 2:
                    Globals.interrupt16h = true;
                    Thread.Sleep(2000);                   
                    break;
                
            }
        }
        private void interruptfunction10h()
        {
            String argument = registergetvalue("AH");
            int argumentvalue = Convert.ToInt32(argument, 2);
            switch (argumentvalue)
            {
                case 11:
                    if (registergetvalue("BH") == "00000000")
                    {
                        String color = registergetvalue("BL");
                        int colorvalue = Convert.ToInt32(color, 2);
                        switch (colorvalue)
                        {
                            case 0: textBox1.BackColor = Color.Black;
                                break;
                            case 1: textBox1.BackColor = Color.Blue;
                                break;
                            case 2: textBox1.BackColor = Color.Green;
                                break;
                            case 3: textBox1.BackColor = Color.Cyan;
                                break;
                            case 4: textBox1.BackColor = Color.Red; 
                                break;
                            case 5: textBox1.BackColor = Color.Magenta;
                                break;
                            case 6: textBox1.BackColor = Color.Brown;
                                break;
                            case 7: textBox1.BackColor = Color.LightGray;
                                break;
                            case 8: textBox1.BackColor = Color.DarkGray;
                                break;
                            case 9: textBox1.BackColor = Color.LightBlue; 
                                break;
                            case 10: textBox1.BackColor = Color.LightGreen;
                                break;
                            case 11: textBox1.BackColor = Color.LightCyan;
                                break;
                            case 12: textBox1.BackColor = Color.LightPink;
                                break;
                            case 13: textBox1.BackColor = Color.LightSteelBlue;
                                break;
                            case 14: textBox1.BackColor = Color.Yellow;
                                break;
                            case 15: textBox1.BackColor = Color.White;
                                break;

                        }
                    }
                    break;

            }
        }
    }
}


