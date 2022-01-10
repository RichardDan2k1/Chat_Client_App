using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;

namespace Chat_Client_App
{
    public partial class Form1 : Form
    {
        Socket sck;
        EndPoint epLocal, epRemote;
        public Form1()
        {
            InitializeComponent();

            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            textLocalIp.Text = GetLocalIP();
            textFriendsIp.Text = GetLocalIP();
        }

        //function to get the local ip
        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            
            foreach(IPAddress ip in host.AddressList)
            {
                if(ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "127.0.0.1";
        }

        //decryption function
        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                int size = sck.EndReceiveFrom(aResult, ref epRemote);

                if (size > 0)
                {
                    byte[] receivedData = new byte[1464];

                    receivedData = (byte[])aResult.AsyncState;

                    //converting byte array to string
                    ASCIIEncoding eEncoding = new ASCIIEncoding();
                    string receivedMessage = eEncoding.GetString(receivedData);

                    //add new decrypt function here should return string
                    string customMessage = customDecryption(receivedMessage);

                    //decrypt-caesar
                    string rMsg = Decipher(customMessage, 3);

                    //idea : could add a random vairable as the key
                    //helps in getting real end to end encryption
                    //session wise and could expire with timer. also new random variable for every message.
                    ChatWindow.Items.Add("Friend : " + rMsg);
                }

                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        //connection function
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                epLocal = new IPEndPoint(IPAddress.Parse(textLocalIp.Text), Convert.ToInt32(textLocalPort.Text));
                sck.Bind(epLocal);

                //connecting local IP with remote IP
                epRemote = new IPEndPoint(IPAddress.Parse(textFriendsIp.Text), Convert.ToInt32(textFriendsPort.Text));
                sck.Connect(epRemote);

                //start listening now
                byte[] buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

                button1.Text = "Connected";
                button1.Enabled = false;
                button2.Enabled = true;
                textBox5.Focus();
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        //encryption function
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(textBox5.Text);

                ASCIIEncoding eEncoding = new ASCIIEncoding();
                string dMsg = eEncoding.GetString(msg);

                //encryption-caesar
                string sMsg = Encipher(dMsg, 3);

                //could add a layer of encryption here
                string customMessage = customEncryption(sMsg);

                //convert to byte array to send through TCP
                byte[] enc_msg = eEncoding.GetBytes(customMessage);

                sck.Send(enc_msg);

                ChatWindow.Items.Add("You : " + textBox5.Text);
                textBox5.Clear();
            }
            catch(Exception ex1)
            {
                MessageBox.Show(ex1.ToString());
            }
        }

        private static char Cipher(char ch, int key)
        {
            if (!char.IsLetter(ch))
                return ch;

            char offset = char.IsUpper(ch) ? 'A' : 'a';
            return (char)((((ch + key) - offset) % 26) + offset);
        }

        public static string Encipher(string input, int key)
        {
            string output = string.Empty;

            foreach (char ch in input)
                output += Cipher(ch, key);

            return output;
        }

        public static string Decipher(string input, int key)
        {
            return Encipher(input, 26 - key);
        }

        // Custom Encryption STARTS ////////////////////////////

        public static string customEncryption(string ss)
        {
            char[] s = ss.ToCharArray();
            int l = s.Length;
            int b = (int)Math.Ceiling(Math.Sqrt(l));
            int a = (int)Math.Floor(Math.Sqrt(l));
            string encrypted = string.Empty;
            if (b * a < l)
            {
                if (Math.Min(b, a) == b)
                {
                    b = b + 1;
                }
                else
                {
                    a = a + 1;
                }
            }
            char[,] arr = new char[a, b];
            int k = 0;

            for (int j = 0; j < a; j++)
            {
                for (int i = 0; i < b; i++)
                {
                    if (k < l)
                    {
                        arr[j, i] = s[k];
                    }
                    k++;
                }
            }

            // Loop to generate
            // encrypted String
            for (int j = 0; j < b; j++)
            {
                for (int i = 0; i < a; i++)
                {
                    encrypted = encrypted +
                                arr[i, j];
                }
            }
            return encrypted;
        }


        public static string customDecryption(string ss)
        {
            char[] s = ss.ToCharArray();
            int l = s.Length;
            int b = (int)Math.Ceiling(Math.Sqrt(l));
            int a = (int)Math.Floor(Math.Sqrt(l));
            string decrypted = string.Empty;

            // Matrix to generate the
            // Encrypted String
            char[,] arr = new char[a, b];
            int k = 0;

            // Fill the matrix column-wise
            for (int j = 0; j < b; j++)
            {
                for (int i = 0; i < a; i++)
                {
                    if (k < l)
                    {
                        arr[j, i] = s[k];
                    }
                    k++;
                }
            }

            // Loop to generate
            // decrypted String
            for (int j = 0; j < a; j++)
            {
                for (int i = 0; i < b; i++)
                {
                    decrypted = decrypted +arr[i, j];
                }
            }
            return decrypted;
        }

        // Custom Encryption ENDS ////////////////////////////
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textLocalIp_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
