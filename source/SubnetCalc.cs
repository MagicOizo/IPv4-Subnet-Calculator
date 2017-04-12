using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.IO;

namespace IPv4SubnetCalculator
{
    public partial class mainForm : Form
    {
        private Dictionary<IPClasses, int> classfullSubnetBits = new Dictionary<IPClasses,int>();
        delegate void SetTextCallback(string text);

        public mainForm()
        {
            InitializeComponent();
            classfullSubnetBits.Add(IPClasses.A, 8);
            classfullSubnetBits.Add(IPClasses.B, 16);
            classfullSubnetBits.Add(IPClasses.C, 24);
            classfullSubnetBits.Add(IPClasses.D, 32);
            classfullSubnetBits.Add(IPClasses.E, 0);
            classfullSubnetBits.Add(IPClasses.unknown, 0);
            for (int maskBits = 1; maskBits <= 32; maskBits++)
            {
                int n = -1; //n = 32Bit set to 1
                n <<= (32 - maskBits);
                string subnet = (n & 255).ToString();
                n >>= 8;
                subnet = (n & 255) + "." + subnet;
                n >>= 8;
                subnet = (n & 255) + "." + subnet;
                n >>= 8;
                subnet = (n & 255) + "." + subnet;
                cboCIDRMask.Items.Add(subnet);
                cboCIDRMaskBits.Items.Add(maskBits.ToString());
                cboAddrMask.Items.Add(subnet);
            }
            lblMask.Visible = false;
        }

        private void cboCIDRMaskBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboCIDRMask.SelectedIndex = cboCIDRMaskBits.SelectedIndex;
            lblMask.Text = "/" + cboCIDRMaskBits.Text;
            lblMask.Visible = true;
            updateSubnetConfigurationComboBoxes();
        }

        private void cboCIDRMask_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboCIDRMaskBits.SelectedIndex = cboCIDRMask.SelectedIndex;
            lblMask.Text = "/" + cboCIDRMaskBits.Text;
            lblMask.Visible = true;
            updateSubnetConfigurationComboBoxes();
        }

        private void updateSubnetConfigurationComboBoxes()
        {
            cboSubnetMask.Items.Clear();
            cboSubnetMaskBits.Items.Clear();
            cboSubnetHostBits.Items.Clear();
            cboSubnets.Items.Clear();
            cboHosts.Items.Clear();
            int hostBits = 0;
            for (int maskBits = cboCIDRMaskBits.SelectedIndex + 1; maskBits <= 32; maskBits++)
            {
                int n = -1; //n = 32Bit set to 1
                n <<= (32 - maskBits);
                string subnet = (n & 255).ToString();
                n >>= 8;
                subnet = (n & 255) + "." + subnet;
                n >>= 8;
                subnet = (n & 255) + "." + subnet;
                n >>= 8;
                subnet = (n & 255) + "." + subnet;
                cboSubnetMask.Items.Add(subnet);
                cboSubnetMaskBits.Items.Add(maskBits.ToString());
                cboSubnets.Items.Add(Math.Pow(2, hostBits).ToString());
                double hostNumber = Math.Pow(2, hostBits);
                if (hostNumber == 1)
                    cboHosts.Items.Add("1");
                else if (hostNumber == 2)
                    cboHosts.Items.Add("2 (RFC3021)");
                else
                    cboHosts.Items.Add((hostNumber - 2).ToString());
                cboSubnetHostBits.Items.Add(hostBits++.ToString());
            }
            cboSubnetMask.SelectedIndex = 0;
            cboSubnetMask_SelectedIndexChanged(cboSubnetMask, new EventArgs());
        }

        private void cboSubnetMask_SelectedIndexChanged(object sender, EventArgs e)
        {      
            cboSubnetMaskBits.SelectedIndex = cboSubnetMask.SelectedIndex;
            cboSubnets.SelectedIndex = cboSubnetMask.SelectedIndex;
            cboSubnetHostBits.SelectedIndex = cboSubnetHostBits.Items.Count - cboSubnetMask.SelectedIndex - 1;
            cboHosts.SelectedIndex = cboHosts.Items.Count - cboSubnetMask.SelectedIndex - 1;
            generateSubnetBitMask(rtbSubnetBitMask, cboCIDRMaskBits.SelectedIndex + 1, cboSubnetMaskBits.SelectedIndex, cboSubnetHostBits.SelectedIndex);
        }

        private void cboSubnetMaskBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboSubnetMask.SelectedIndex = cboSubnetMaskBits.SelectedIndex;
            cboSubnets.SelectedIndex = cboSubnetMaskBits.SelectedIndex;
            cboSubnetHostBits.SelectedIndex = cboSubnetHostBits.Items.Count - cboSubnetMaskBits.SelectedIndex - 1;
            cboHosts.SelectedIndex = cboHosts.Items.Count - cboSubnetMaskBits.SelectedIndex - 1;
            generateSubnetBitMask(rtbSubnetBitMask, cboCIDRMaskBits.SelectedIndex + 1, cboSubnetMaskBits.SelectedIndex, cboSubnetHostBits.SelectedIndex);
        }

        private void cboSubnetHostBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboSubnetMaskBits.SelectedIndex = cboSubnetMaskBits.Items.Count - cboSubnetHostBits.SelectedIndex - 1;
            cboSubnets.SelectedIndex = cboSubnets.Items.Count - cboSubnetHostBits.SelectedIndex - 1;
            cboSubnetMask.SelectedIndex = cboSubnetMask.Items.Count - cboSubnetHostBits.SelectedIndex - 1;
            cboHosts.SelectedIndex = cboSubnetHostBits.SelectedIndex;
            generateSubnetBitMask(rtbSubnetBitMask, cboCIDRMaskBits.SelectedIndex +1, cboSubnetMaskBits.SelectedIndex, cboSubnetHostBits.SelectedIndex);
        }

        private void cboSubnets_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboSubnetMask.SelectedIndex = cboSubnets.SelectedIndex;
            cboSubnetMaskBits.SelectedIndex = cboSubnets.SelectedIndex;
            cboSubnetHostBits.SelectedIndex = cboSubnetHostBits.Items.Count - cboSubnets.SelectedIndex - 1;
            cboHosts.SelectedIndex = cboHosts.Items.Count - cboSubnets.SelectedIndex - 1;
            generateSubnetBitMask(rtbSubnetBitMask, cboCIDRMaskBits.SelectedIndex +1, cboSubnetMaskBits.SelectedIndex, cboSubnetHostBits.SelectedIndex);
        }

        private void cboHosts_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboSubnetMaskBits.SelectedIndex = cboSubnetMaskBits.Items.Count - cboHosts.SelectedIndex - 1;
            cboSubnets.SelectedIndex = cboSubnets.Items.Count - cboHosts.SelectedIndex - 1;
            cboSubnetMask.SelectedIndex = cboSubnetMask.Items.Count - cboHosts.SelectedIndex - 1;
            cboSubnetHostBits.SelectedIndex = cboHosts.SelectedIndex;
            if (cboHosts.SelectedIndex == 1)
                linkRfcLink.Visible = true;
            else
                linkRfcLink.Visible = false;
            generateSubnetBitMask(rtbSubnetBitMask, cboCIDRMaskBits.SelectedIndex + 1, cboSubnetMaskBits.SelectedIndex, cboSubnetHostBits.SelectedIndex);
        }

        private void linkRfcLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.ietf.org/rfc/rfc3021.txt");
        }

        private void generateSubnetBitMask(RichTextBox RichTextBox, int maskBits, int subnetBits, int hostBits)
        {
            generateSubnetBitMask(RichTextBox, maskBits, subnetBits, hostBits, IPClasses.unknown);    
        }

        private void generateSubnetBitMask(RichTextBox outputRichTextBox,int maskBits, int subnetBits, int hostBits, IPClasses IPClass)
        {
            if (maskBits + subnetBits + hostBits != 32)
                return;
            RichTextBox RichTextBox = new RichTextBox();
            RichTextBox.Font = outputRichTextBox.Font;
            string subnetBitMask = "";
            int bitPos = 0;
            int partStart = 0;
            int partEnd = 0;
            Dictionary<int[], Color> colorizing = new Dictionary<int[], Color>();
            if (IPClass == IPClasses.A)
            {
                maskBits -= 1;
                subnetBitMask += "0";
                colorizing.Add(new int[] { 0,0},Color.Orange);
                partStart = 1;
                bitPos = 1;
            }
            else if (IPClass == IPClasses.B)
            {
                maskBits -= 2;
                subnetBitMask += "10";
                colorizing.Add(new int[] { 0, 1 }, Color.Orange);
                partStart = 2;
                bitPos = 2;
            }
            else if (IPClass == IPClasses.C)
            {
                maskBits -= 3;
                subnetBitMask += "110";
                colorizing.Add(new int[] { 0, 2 }, Color.Orange);
                partStart = 3;
                bitPos = 3;
            }
            if (IPClass == IPClasses.D)
            {
                RichTextBox.Text = "1110nnnn.nnnnnnnn.nnnnnnnn.nnnnnnnn";
                colorizing.Add(new int[] { 0, 3 }, Color.Orange);
                colorizing.Add(new int[] { 4, 34 }, Color.Blue);
                colorizing.Add(new int[] { 8, 8 }, Color.Black);
                colorizing.Add(new int[] { 17, 17 }, Color.Black);
                colorizing.Add(new int[] { 26, 26 }, Color.Black);
            }
            else if (IPClass == IPClasses.E)
            {
                RichTextBox.Text = "1111XXXX.XXXXXXXX.XXXXXXXX.XXXXXXXX";
                colorizing.Add(new int[] { 0, 3 }, Color.Orange);
                colorizing.Add(new int[] { 4, 34 }, Color.Magenta);
                colorizing.Add(new int[] { 8, 8 }, Color.Black);
                colorizing.Add(new int[] { 17, 17 }, Color.Black);
                colorizing.Add(new int[] { 26, 26 }, Color.Black);
            }
            else
            {
                for (int i = 0; i < maskBits; i++)
                {
                    if (bitPos++ == 8)
                    {
                        partEnd = subnetBitMask.Length - 1;
                        colorizing.Add(new int[] { partStart, partEnd }, Color.Blue);
                        colorizing.Add(new int[] { partEnd + 1, partEnd + 1 }, Color.Black);
                        bitPos = 1;
                        subnetBitMask += ".";
                        partStart = subnetBitMask.Length;
                    }
                    subnetBitMask += "n";
                }
                partEnd = subnetBitMask.Length - 1;
                colorizing.Add(new int[] { partStart, partEnd }, Color.Blue);
                partStart = subnetBitMask.Length;
                for (int i = 0; i < subnetBits; i++)
                {
                    if (bitPos++ == 8)
                    {
                        partEnd = subnetBitMask.Length - 1;
                        if (partEnd >= partStart)
                            colorizing.Add(new int[] { partStart, partEnd }, Color.Red);
                        colorizing.Add(new int[] { partEnd + 1, partEnd + 1 }, Color.Black);
                        bitPos = 1;
                        subnetBitMask += ".";
                        partStart = subnetBitMask.Length;
                    }
                    subnetBitMask += "s";
                }
                if (subnetBits > 0)
                {
                    partEnd = subnetBitMask.Length - 1;
                    colorizing.Add(new int[] { partStart, partEnd }, Color.Red);
                    partStart = subnetBitMask.Length;
                }
                for (int i = 0; i < hostBits; i++)
                {
                    if (bitPos++ == 8)
                    {
                        partEnd = subnetBitMask.Length - 1;
                        if (partEnd >= partStart)
                            colorizing.Add(new int[] { partStart, partEnd }, Color.Green);
                        colorizing.Add(new int[] { partEnd + 1, partEnd + 1 }, Color.Black);
                        bitPos = 1;
                        subnetBitMask += ".";
                        partStart = subnetBitMask.Length;
                    }
                    subnetBitMask += "h";
                }
                if (hostBits > 0)
                {
                    partEnd = subnetBitMask.Length - 1;
                    colorizing.Add(new int[] { partStart, partEnd }, Color.Green);
                }
                RichTextBox.Text = subnetBitMask;
                Application.DoEvents();
            }
            foreach (KeyValuePair<int[], Color> part in colorizing)
            {
                RichTextBox.Select(part.Key[0], part.Key[1] - part.Key[0] +1 );
                RichTextBox.SelectionColor = part.Value;
            }
            outputRichTextBox.Rtf = RichTextBox.Rtf;
        }

        private void btnGenerateSubnets_Click(object sender, EventArgs e)
        {
            btnGenerateSubnets.Enabled = false;
            barProgress.Value = 0;
            dgrSubnets.Visible = false;
            DataTable tblSubnets = new DataTable();
            dgrSubnets.DataSource = null;
            Regex regex = new Regex(@"^([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])$");
            if (!regex.IsMatch(txtIPAdd.Text))
            {
                MessageBox.Show("Address block contains no valid IP address", "No Valid Address Block", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string[] temp = txtIPAdd.Text.Split(new char[] { '.' });
            int[] IPAddress = new int[4];
            for (int i = 0; i < 4; i++)
                IPAddress[i] = Convert.ToInt32(temp[i]);
            
            if (cboCIDRMask.SelectedIndex<0)
            {
                MessageBox.Show("CIDR mask contains no valid IP mask", "No Valid CIDR Mask", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            temp = cboCIDRMask.Text.Split(new char[] { '.' });
            int[] CIDRMask = new int[4];
            for (int i = 0; i < 4; i++)
                CIDRMask[i] = Convert.ToInt32(temp[i]);
            for (int i = 0; i < 4; i++)
                IPAddress[i] = IPAddress[i] & CIDRMask[i];
            int anzahlSubnets = (int)Math.Pow(2, cboSubnetMaskBits.SelectedIndex);
            int subnetSize = (int)Math.Pow(2, cboSubnetHostBits.SelectedIndex);
            dgrSubnets.DataSource = tblSubnets.DefaultView;
            if (anzahlSubnets > 32766 && MessageBox.Show("There will be " + anzahlSubnets + " Subnets calculated. That may take some time.\r\nDo you still want to calculate the subnets?", "Big Amount of Subnets to Calculate", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
            {
                barProgress.Visible = false;
                dgrSubnets.Visible = true;
                btnGenerateSubnets.Enabled = true;
                return;
            }
            tblSubnets.Columns.Add("Subnet");
            tblSubnets.Columns.Add("Mask");
            tblSubnets.Columns.Add("Subnet Size");
            tblSubnets.Columns.Add("Host Range");
            tblSubnets.Columns.Add("Broadcast");
            dgrSubnets.Columns[0].Width = 110;
            dgrSubnets.Columns[1].Width = 110;
            dgrSubnets.Columns[2].Width = 80;
            int binaryAddress = 0;
            foreach (int part in IPAddress)
            {
                binaryAddress <<= 8;
                binaryAddress += part;
            }
            //Console.WriteLine(Convert.ToString(binaryAddress, 2));
            barProgress.Maximum = anzahlSubnets;
            barProgress.Visible = true;
            for (int subnet = 0; subnet < anzahlSubnets; subnet++)
            {
                string subnetAddr = convertToIP(binaryAddress);
                string hostSize = subnetSize.ToString();
                if (subnetSize > 2)
                    hostSize = (subnetSize - 2).ToString();
                string firstAddr = "";
                string lastAddr = "";
                if(subnetSize == 1)
                {
                    firstAddr = convertToIP(binaryAddress);
                    lastAddr = convertToIP(binaryAddress);
                }
                else if (subnetSize == 2)
                {
                    firstAddr = convertToIP(binaryAddress);
                    lastAddr = convertToIP(++binaryAddress);
                }
                else
                {
                    firstAddr = convertToIP(++binaryAddress);
                    binaryAddress += subnetSize - 3;
                    lastAddr = convertToIP(binaryAddress);
                }
                string broadcast = "";
                if (subnetSize < 4)
                    broadcast = convertToIP(binaryAddress);
                else
                    broadcast= convertToIP(++binaryAddress);
                tblSubnets.Rows.Add(new string[] { subnetAddr, cboSubnetMask.Text, hostSize, firstAddr + " to " + lastAddr , broadcast });
                binaryAddress++;
                barProgress.Value = subnet;
                Application.DoEvents();
            }
            barProgress.Visible = false;
            dgrSubnets.Visible = true;
            btnGenerateSubnets.Enabled = true;
        }

        private string convertToIP(int binaryIP)
        {
            string output = (binaryIP & 255).ToString();
            binaryIP >>= 8;
            output = (binaryIP & 255) + "." + output;
            binaryIP >>= 8;
            output = (binaryIP & 255) + "." + output;
            binaryIP >>= 8;
            output = (binaryIP & 255) + "." + output;
            return output;
        }

        private void cmdAddrGenerateAddresses_Click(object sender, EventArgs e)
        {
            cmdAddrGenerateAddresses.Enabled = false;
            barAddrProgress.Value = 0;
            dgrAddresses.Visible = false;
            DataTable tblAddresses = new DataTable();
            dgrAddresses.DataSource = null;
            Regex regex = new Regex(@"^([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])$");
            if (!regex.IsMatch(txtAddrIPAddress.Text))
            {
                MessageBox.Show("IP Address contains no valid IP address", "No Valid IP Address", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string[] temp = txtAddrIPAddress.Text.Split(new char[] { '.' });
            int[] IPAddress = new int[4];
            for (int i = 0; i < 4; i++)
                IPAddress[i] = Convert.ToInt32(temp[i]);

            if (cboAddrMask.SelectedIndex < 0)
            {
                MessageBox.Show("Subnet mask contains no valid IP mask", "No Valid Subnet Mask", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            temp = cboAddrMask.Text.Split(new char[] { '.' });
            int[] subnetMask = new int[4];
            for (int i = 0; i < 4; i++)
                subnetMask[i] = Convert.ToInt32(temp[i]);
            for (int i = 0; i < 4; i++)
                IPAddress[i] = IPAddress[i] & subnetMask[i];
            int subnetSize = (int)Math.Pow(2, (32 - cboAddrMask.SelectedIndex - 1));
            dgrAddresses.DataSource = tblAddresses.DefaultView;
            if (subnetSize > 32766 && MessageBox.Show("There will be " + subnetSize + " Hosts displayed. That may take some time.\r\nDo you still want to display the hosts?", "Big Amount of Hosts to Display", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
            {
                barAddrProgress.Visible = false;
                dgrAddresses.Visible = true;
                cmdAddrGenerateAddresses.Enabled = true;
                return;
            }
            tblAddresses.Columns.Add("IP");
            tblAddresses.Columns.Add("Mask");
            tblAddresses.Columns.Add("Notes");
            dgrAddresses.Columns[0].Width = 110;
            dgrAddresses.Columns[1].Width = 110;
            int binaryAddress = 0;
            foreach (int part in IPAddress)
            {
                binaryAddress <<= 8;
                binaryAddress += part;
            }
            //Console.WriteLine(Convert.ToString(binaryAddress, 2));
            barAddrProgress.Maximum = subnetSize;
            barAddrProgress.Visible = true;
            for (int address = 0; address < subnetSize; address++)
            {
                string ipAddr = convertToIP(binaryAddress++);
                string note = "";
                if (subnetSize == 1)
                    note = "Single host address";
                else if (address == 0)
                    note = "Subnet address";
                else if (address == subnetSize - 1)
                    note = "Broadcast address";
                else if ((ipAddr.EndsWith(".0") || ipAddr.EndsWith(".255")) && subnetSize > 2)
                    note = "Suggestion: Don't use due to numbering convention";
                tblAddresses.Rows.Add(new string[] { ipAddr, cboAddrMask.Text, note });
                barAddrProgress.Value = address;
                Application.DoEvents();
            }
            barAddrProgress.Visible = false;
            dgrAddresses.Visible = true;
            cmdAddrGenerateAddresses.Enabled = true;
        }

        private void txtClassfullAddr_TextChanged(object sender, EventArgs e)
        {
            cboClassfullHostBits.Items.Clear();
            cboClassfullHosts.Items.Clear();
            cboClassfullMask.Items.Clear();
            cboClassfullMaskBits.Items.Clear();
            cboClassfullSubnets.Items.Clear();
            rtbClassfullBitMask.Text = "";
            if (findIPClass(txtClassfullAddr.Text) != IPClasses.unknown && findIPClass(txtClassfullAddr.Text) != IPClasses.E)
            {
                int hostBits = 0;
                for (int maskBits = classfullSubnetBits[findIPClass(txtClassfullAddr.Text)]; maskBits <= 32; maskBits++)
                {
                    int n = -1; //n = 32Bit set to 1
                    n <<= (32 - maskBits);
                    string subnet = (n & 255).ToString();
                    n >>= 8;
                    subnet = (n & 255) + "." + subnet;
                    n >>= 8;
                    subnet = (n & 255) + "." + subnet;
                    n >>= 8;
                    subnet = (n & 255) + "." + subnet;
                    cboClassfullMask.Items.Add(subnet);
                    cboClassfullMaskBits.Items.Add(maskBits.ToString());
                    cboClassfullSubnets.Items.Add(Math.Pow(2, hostBits).ToString());
                    double hostNumber = Math.Pow(2, hostBits);
                    if (hostNumber == 1)
                        cboClassfullHosts.Items.Add("1");
                    else if (hostNumber == 2)
                        cboClassfullHosts.Items.Add("2 (RFC3021)");
                    else
                        cboClassfullHosts.Items.Add((hostNumber - 2).ToString());
                    cboClassfullHostBits.Items.Add(hostBits++.ToString());
                }
                cboClassfullMask.SelectedIndex = 0;
                cboClassfullMask_SelectedIndexChanged(cboClassfullMask, new EventArgs());
            }
        }

        private enum IPClasses
        {
            A,
            B,
            C,
            D,
            E,
            unknown
        }

        private IPClasses findIPClass(string IPAddress)
        {
            Regex regex = new Regex(@"^([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])$");
            if (regex.IsMatch(IPAddress))
            {
                string[] temp = IPAddress.Split(new char[] { '.' });
                int[] iIPAddress = new int[4];
                for (int i = 0; i < 4; i++)
                    iIPAddress[i] = Convert.ToInt32(temp[i]);
                int iBinaryAddress = 0;
                foreach (int part in iIPAddress)
                {
                    iBinaryAddress <<= 8;
                    iBinaryAddress += part;
                }
                string sBinaryAddress = Convert.ToString(iBinaryAddress, 2);
                if (sBinaryAddress.Length < 32)
                {
                    while(sBinaryAddress.Length < 32)
                        sBinaryAddress = "0" + sBinaryAddress;
                }
                if (sBinaryAddress.StartsWith("0"))
                    return IPClasses.A;
                else if (sBinaryAddress.StartsWith("10"))
                    return IPClasses.B;
                else if (sBinaryAddress.StartsWith("110"))
                    return IPClasses.C;
                else if (sBinaryAddress.StartsWith("1110"))
                    return IPClasses.D;
                else if (sBinaryAddress.StartsWith("1111"))
                    return IPClasses.E;
            }
            return IPClasses.unknown;
        }

        private void cboClassfullMask_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboClassfullMaskBits.SelectedIndex = cboClassfullMask.SelectedIndex;
            cboClassfullSubnets.SelectedIndex = cboClassfullMask.SelectedIndex;
            cboClassfullHostBits.SelectedIndex = cboClassfullHostBits.Items.Count - cboClassfullMask.SelectedIndex - 1;
            cboClassfullHosts.SelectedIndex = cboClassfullHosts.Items.Count - cboClassfullMask.SelectedIndex - 1;
            generateSubnetBitMask(rtbClassfullBitMask, classfullSubnetBits[findIPClass(txtClassfullAddr.Text)], cboClassfullMaskBits.SelectedIndex, cboClassfullHostBits.SelectedIndex, findIPClass(txtClassfullAddr.Text));
        }

        private void cboClassfullMaskBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboClassfullMask.SelectedIndex = cboClassfullMaskBits.SelectedIndex;
            cboClassfullSubnets.SelectedIndex = cboClassfullMaskBits.SelectedIndex;
            cboClassfullHostBits.SelectedIndex = cboClassfullHostBits.Items.Count - cboClassfullMaskBits.SelectedIndex - 1;
            cboClassfullHosts.SelectedIndex = cboClassfullHosts.Items.Count - cboClassfullMaskBits.SelectedIndex - 1;
            generateSubnetBitMask(rtbClassfullBitMask, classfullSubnetBits[findIPClass(txtClassfullAddr.Text)], cboClassfullMaskBits.SelectedIndex, cboClassfullHostBits.SelectedIndex, findIPClass(txtClassfullAddr.Text));
        }

        private void cboClassfullHostBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboClassfullMaskBits.SelectedIndex = cboClassfullMaskBits.Items.Count - cboClassfullHostBits.SelectedIndex - 1;
            cboClassfullSubnets.SelectedIndex = cboClassfullSubnets.Items.Count - cboClassfullHostBits.SelectedIndex - 1;
            cboClassfullMask.SelectedIndex = cboClassfullMask.Items.Count - cboClassfullHostBits.SelectedIndex - 1;
            cboClassfullHosts.SelectedIndex = cboClassfullHostBits.SelectedIndex;
            generateSubnetBitMask(rtbClassfullBitMask, classfullSubnetBits[findIPClass(txtClassfullAddr.Text)], cboClassfullMaskBits.SelectedIndex, cboClassfullHostBits.SelectedIndex, findIPClass(txtClassfullAddr.Text));
        }

        private void cboClassfullSubnets_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboClassfullMask.SelectedIndex = cboClassfullSubnets.SelectedIndex;
            cboClassfullMaskBits.SelectedIndex = cboClassfullSubnets.SelectedIndex;
            cboClassfullHostBits.SelectedIndex = cboClassfullHostBits.Items.Count - cboClassfullSubnets.SelectedIndex - 1;
            cboClassfullHosts.SelectedIndex = cboClassfullHosts.Items.Count - cboClassfullSubnets.SelectedIndex - 1;
            generateSubnetBitMask(rtbClassfullBitMask, classfullSubnetBits[findIPClass(txtClassfullAddr.Text)], cboClassfullMaskBits.SelectedIndex, cboClassfullHostBits.SelectedIndex, findIPClass(txtClassfullAddr.Text));
        }

        private void cboClassfullHosts_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboClassfullMaskBits.SelectedIndex = cboClassfullMaskBits.Items.Count - cboClassfullHosts.SelectedIndex - 1;
            cboClassfullSubnets.SelectedIndex = cboClassfullSubnets.Items.Count - cboClassfullHosts.SelectedIndex - 1;
            cboClassfullMask.SelectedIndex = cboClassfullMask.Items.Count - cboClassfullHosts.SelectedIndex - 1;
            cboClassfullHostBits.SelectedIndex = cboClassfullHosts.SelectedIndex;
            if (cboClassfullHosts.SelectedIndex == 1)
                lklClassfullRFC.Visible = true;
            else
                lklClassfullRFC.Visible = false;
            generateSubnetBitMask(rtbClassfullBitMask, classfullSubnetBits[findIPClass(txtClassfullAddr.Text)], cboClassfullMaskBits.SelectedIndex, cboClassfullHostBits.SelectedIndex, findIPClass(txtClassfullAddr.Text));
        }

        private void cmdClassfullCalculate_Click(object sender, EventArgs e)
        {
            cmdClassfullCalculate.Enabled = false;
            barClassfullProgress.Value = 0;
            dgrClassfullSubnets.Visible = false;
            DataTable tblSubnets = new DataTable();
            dgrClassfullSubnets.DataSource = null;
            Regex regex = new Regex(@"^([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])$");
            if (!regex.IsMatch(txtClassfullAddr.Text))
            {
                MessageBox.Show("Address block contains no valid IP address", "No Valid Address Block", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string[] temp = txtClassfullAddr.Text.Split(new char[] { '.' });
            int[] IPAddress = new int[4];
            for (int i = 0; i < 4; i++)
                IPAddress[i] = Convert.ToInt32(temp[i]);

            if (cboClassfullMask.SelectedIndex < 0)
            {
                MessageBox.Show("CIDR mask contains no valid IP mask", "No Valid CIDR Mask", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            temp = cboClassfullMask.Text.Split(new char[] { '.' });
            int[] ClassfullMask = new int[4];
            for (int i = 0; i < 4; i++)
                ClassfullMask[i] = Convert.ToInt32(temp[i]);
            for (int i = 0; i < 4; i++)
                IPAddress[i] = IPAddress[i] & ClassfullMask[i];
            int anzahlSubnets = (int)Math.Pow(2, cboClassfullMaskBits.SelectedIndex);
            int subnetSize = (int)Math.Pow(2, cboClassfullHostBits.SelectedIndex);
            dgrClassfullSubnets.DataSource = tblSubnets.DefaultView;
            if (anzahlSubnets > 32766 && MessageBox.Show("There will be " + anzahlSubnets + " Subnets calculated. That may take some time.\r\nDo you still want to calculate the subnets?", "Big Amount of Subnets to Calculate", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
            {
                barClassfullProgress.Visible = false;
                dgrClassfullSubnets.Visible = true;
                cmdClassfullCalculate.Enabled = true;
                return;
            }
            tblSubnets.Columns.Add("Subnet");
            tblSubnets.Columns.Add("Mask");
            tblSubnets.Columns.Add("Subnet Size");
            tblSubnets.Columns.Add("Host Range");
            tblSubnets.Columns.Add("Broadcast");
            dgrClassfullSubnets.Columns[0].Width = 110;
            dgrClassfullSubnets.Columns[1].Width = 110;
            dgrClassfullSubnets.Columns[2].Width = 80;
            int binaryAddress = 0;
            foreach (int part in IPAddress)
            {
                binaryAddress <<= 8;
                binaryAddress += part;
            }
            //Console.WriteLine(Convert.ToString(binaryAddress, 2));
            barClassfullProgress.Maximum = anzahlSubnets;
            barClassfullProgress.Visible = true;
            for (int subnet = 0; subnet < anzahlSubnets; subnet++)
            {
                string subnetAddr = convertToIP(binaryAddress);
                string hostSize = subnetSize.ToString();
                if (subnetSize > 2)
                    hostSize = (subnetSize - 2).ToString();
                string firstAddr = "";
                string lastAddr = "";
                if (subnetSize == 1)
                {
                    firstAddr = convertToIP(binaryAddress);
                    lastAddr = convertToIP(binaryAddress);
                }
                else if (subnetSize == 2)
                {
                    firstAddr = convertToIP(binaryAddress);
                    lastAddr = convertToIP(++binaryAddress);
                }
                else
                {
                    firstAddr = convertToIP(++binaryAddress);
                    binaryAddress += subnetSize - 3;
                    lastAddr = convertToIP(binaryAddress);
                }
                string broadcast = "";
                if (subnetSize < 4)
                    broadcast = convertToIP(binaryAddress);
                else
                    broadcast = convertToIP(++binaryAddress);
                tblSubnets.Rows.Add(new string[] { subnetAddr, cboClassfullMask.Text, hostSize, firstAddr + " to " + lastAddr, broadcast });
                binaryAddress++;
                barClassfullProgress.Value = subnet;
                Application.DoEvents();
            }
            barClassfullProgress.Visible = false;
            dgrClassfullSubnets.Visible = true;
            cmdClassfullCalculate.Enabled = true;
        }

        private void dgrClassfullSubnets_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            txtAddrIPAddress.Text = dgrClassfullSubnets.Rows[e.RowIndex].Cells[0].Value.ToString();
            cboAddrMask.Text = dgrClassfullSubnets.Rows[e.RowIndex].Cells[1].Value.ToString();
            tabAuswahl.SelectedTab = tabAddrs;
            cmdAddrGenerateAddresses_Click(cmdAddrGenerateAddresses, new EventArgs());
        }

        private void dgrSubnets_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            txtAddrIPAddress.Text = dgrSubnets.Rows[e.RowIndex].Cells[0].Value.ToString();
            cboAddrMask.Text = dgrSubnets.Rows[e.RowIndex].Cells[1].Value.ToString();
            tabAuswahl.SelectedTab = tabAddrs;
            cmdAddrGenerateAddresses_Click(cmdAddrGenerateAddresses, new EventArgs());
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:max@zoeller.me");
        }

        private DateTime startResolve;
        private IAsyncResult resolveResult;

        private void cmdLookupIP_Click(object sender, EventArgs e)
        {
            cmdLookupHostname.Enabled = false;
            cmdLookupIP.Enabled = false;
            chkWhoisLookup.Enabled = false;
            txtLookupIP.Text = "";
            txtHex.Text = "";
            txtBin.Text = "";
            txtDetails.Text = "";
            txtResponse.Text = "";
            startResolve = DateTime.Now;
            try
            {
                resolveResult = Dns.BeginGetHostAddresses(txtLookupHostname.Text, new AsyncCallback(hostnameLookupCallback), txtLookupHostname);
                trmResponseTimeout.Start();
            }
            catch(ArgumentNullException)
            {
                txtResponse.Text = "Resolve not possible: No Hostname address given for lookup!";
                updateLookup();
            }
            catch (ArgumentOutOfRangeException)
            {
                txtResponse.Text = "Resolve not possible: Given Hostname is longer than 255 characters!";
                updateLookup();
            }
            catch (ArgumentException)
            {
                txtResponse.Text = "Resolve not possible: Given IP address is not a valid IPv4 address!";
                updateLookup();
            }
            catch (Exception exception)
            {
                txtResponse.Text = "Resolve not possible because exception occured: " + exception.Message + "\r\n" + exception.StackTrace;
                updateLookup();
            }
        }

        private void cmdLookupHostname_Click(object sender, EventArgs e)
        {
            cmdLookupHostname.Enabled = false;
            cmdLookupIP.Enabled = false;
            chkWhoisLookup.Enabled = false;
            txtLookupHostname.Text = "";
            txtHex.Text = "";
            txtBin.Text = "";
            txtDetails.Text = "";
            txtResponse.Text = "";
            startResolve = DateTime.Now;
            try
            {
                resolveResult = Dns.BeginGetHostEntry(new IPAddress(ipStringToByteArray(txtLookupIP.Text)), new AsyncCallback(ipLookupCallback), txtLookupHostname);
                trmResponseTimeout.Start();
            }
            catch (ArgumentNullException)
            {
                txtResponse.Text = "Resolve not possible: No IPv4 address given for lookup!";
                updateLookup();
            }
            catch (ArgumentOutOfRangeException)
            {
                txtResponse.Text = "Resolve not possible: Given Hostname is longer than 255 characters!";
                updateLookup();
            }
            catch (ArgumentException)
            {
                txtResponse.Text = "Resolve not possible: Given IP address is not a valid IPv4 address!";
                updateLookup();
            }
            catch (Exception exception)
            {
                txtResponse.Text = "Resolve not possible because exception occured: " + exception.Message + "\r\n" + exception.StackTrace;
                updateLookup();
            }
        }

        private void hostnameLookupCallback(IAsyncResult result)
        {
            try
            {
                IPAddress[] addressList = Dns.EndGetHostAddresses(result);
                if(addressList.Length == 0)
                {
                    setTxtResponseError("Lookup responded no IP address.");
                }
                foreach (var address in addressList)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        setTxtLookupIP(address.ToString());
                        return;
                    }
                }
                setTxtResponseError("Lookup responded no IPv4 address.");
            }
            catch
            {
                setTxtResponseError("Lookup not successfull");
            }
        }

        private void ipLookupCallback(IAsyncResult result)
        {
            try
            {
                IPHostEntry hostEntry = Dns.EndGetHostEntry(result);
                setTxtLookupHostname(hostEntry.HostName);
            }
            catch
            {
                setTxtResponseError("Lookup not successfull");
            }
        }


        private void setTxtLookupIP(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.txtLookupIP.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setTxtLookupIP);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                DateTime endTime = DateTime.Now;
                TimeSpan resolveTime = endTime.Subtract(startResolve);
                this.txtLookupIP.Text = text;
                this.txtResponse.Text = resolveTime.Milliseconds + " Milliseconds";
                updateLookup();
            }
        }

        private void setTxtLookupHostname(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.txtLookupIP.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setTxtLookupHostname);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                DateTime endTime = DateTime.Now;
                TimeSpan resolveTime = endTime.Subtract(startResolve);
                this.txtLookupHostname.Text = text;
                this.txtResponse.Text = resolveTime.Milliseconds + " Milliseconds";
                updateLookup();
            }
        }

        private void setTxtResponseError(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.txtLookupIP.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setTxtResponseError);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.txtResponse.Text = text;
                updateLookup();
            }
        }

        private byte[] ipStringToByteArray(string IPAddress)
        {
            byte[] output = new byte[0];
            Regex regex = new Regex(@"^([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])$");
            if (regex.IsMatch(IPAddress))
            {
                output = new byte[4];
                string[] temp = IPAddress.Split(new char[] { '.' });
                for (int i = 0; i < 4; i++)
                    output[i] = Convert.ToByte(temp[i]);
            }
            return output;
        }

        private void updateLookup()
        {
            byte[] ipAddress = ipStringToByteArray(txtLookupIP.Text);
            txtBin.Text = "";
            txtHex.Text = "";
            txtDetails.Text = "";
            if (ipAddress.Length == 4)
            {
                foreach (byte part in ipAddress)
                {
                    string temp = Convert.ToString(part, 2);
                    while (temp.Length < 8)
                        temp = "0" + temp;
                    txtBin.Text += temp + ".";
                    temp = Convert.ToString(part, 16);
                    while (temp.Length < 2)
                        temp = "0" + temp;
                    txtHex.Text += temp + ".";
                }
                txtBin.Text = txtLookupIP.Text + " = " + txtBin.Text.Substring(0, 35);
                txtHex.Text = txtLookupIP.Text + " = " + txtHex.Text.Substring(0, 11).ToUpper();
                RichTextBox tempRTB = new RichTextBox();
                generateSubnetBitMask(tempRTB, classfullSubnetBits[findIPClass(txtLookupIP.Text)], 0, 32 - classfullSubnetBits[findIPClass(txtLookupIP.Text)], findIPClass(txtLookupIP.Text));
                if (findIPClass(txtLookupIP.Text) != IPClasses.unknown)
                    txtDetails.Text = "IP Class \"" + findIPClass(txtLookupIP.Text) + "\"\r\n";
                else
                    txtDetails.Text = "Unknown IP range\r\n";
                txtDetails.Text += tempRTB.Text;
                if (findIPClass(txtLookupIP.Text) == IPClasses.D)
                    txtDetails.Text += "\r\nMulticast addresses";
                if (findIPClass(txtLookupIP.Text) == IPClasses.E)
                    txtDetails.Text += "\r\nReserved address range for special purposes";
                if(descibePrivateIPRange(txtLookupIP.Text) != "")
                    txtDetails.Text += "\r\n" + descibePrivateIPRange(txtLookupIP.Text);
            }
            if(chkWhoisLookup.Checked)
            { 
                if(txtLookupHostname.Text != string.Empty)
                {
                    txtDetails.Text += WhoIs(txtLookupHostname.Text);
                }
                else if (txtLookupIP.Text != string.Empty)
                {
                    txtDetails.Text += WhoIs(txtLookupIP.Text);
                }
            }
            cmdLookupHostname.Enabled = true;
            cmdLookupIP.Enabled = true;
            chkWhoisLookup.Enabled = true;
            trmResponseTimeout.Stop();
        }

        public string WhoIs(string address)
        {
            return WhoIs(address, "whois.iana.org");
        }

        public string WhoIs(string address, string server)
        {
            Regex findRefer = new Regex(@"^refer:(\s*)(.*)$", RegexOptions.Multiline);
            StringBuilder response = new StringBuilder();
            response.Append("\r\n\r\n======= WHOIS (" + server + ")=======\r\n\r\n");
            if (server == null || server == string.Empty)
                return null;
            if (server == "whois.denic.de")
                address = "-T dn,ace " + address;
            int port = 43;
            using (var client = new TcpClient(server, port))
            {
                using (var clientStream = client.GetStream())
                {
                    using (var buffer = new BufferedStream(clientStream))
                    {
                        var streamWriter = new StreamWriter(buffer);
                        streamWriter.WriteLine(address);
                        streamWriter.Flush();
                        var streamReader = new StreamReader(buffer);
                        response.Append(streamReader.ReadToEnd());
                    }
                    clientStream.Close();
                    clientStream.Dispose();
                }
                client.Close();
            }
            if(findRefer.IsMatch(response.ToString()))
            {
                Match referMatch = findRefer.Match(response.ToString());
                if (referMatch.Groups.Count >= 3)
                {
                    string referServer = findRefer.Match(response.ToString()).Groups[2].Value;
                    response.Insert(0,WhoIs(address, referServer));
                }
            }
            return response.ToString();
        }

        private void trmResponseTimeout_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine(resolveResult.AsyncState.ToString());
            if (!resolveResult.IsCompleted)
            {
                if (resolveResult.AsyncState == txtLookupHostname)
                {
                    Dns.EndGetHostAddresses(resolveResult);
                }
                else
                {
                    Dns.EndGetHostEntry(resolveResult);
                }
                txtResponse.Text = "Response Timeout";
            }
            updateLookup();
        }

        private string descibePrivateIPRange(string ipAddress)
        {
            Regex regex = new Regex(@"^([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])\.([01]?\d\d?|2[0-4]\d|25[0-5])$");
            if (regex.IsMatch(ipAddress))
            {
                string[] temp = ipAddress.Split(new char[] { '.' });
                int[] iIPAddress = new int[4];
                for (int i = 0; i < 4; i++)
                    iIPAddress[i] = Convert.ToInt32(temp[i]);
                int[] netAddress = new int[4]; //Look for reservations as in rfc6890
                if (addressIsMatch(getQuadInt(0, 0, 0, 0), 8, iIPAddress))
                    return "Reserved for self-identification\r\n(0.0.0.0 to 0.255.255.255) as described in RFC1122 (https://tools.ietf.org/html/rfc1122)";
                if (addressIsMatch(getQuadInt(10, 0, 0, 0), 8, iIPAddress))
                    return "Reserved for use in private networks only\r\n(10.0.0.0 to 10.255.255.255) as described in RFC1918 (https://tools.ietf.org/html/rfc1918)";
                if (addressIsMatch(getQuadInt(100, 64, 0, 0), 10, iIPAddress))
                    return "Reserved for Shared Address Space\r\n(100.64.0.0 to 100.127.255.255) as described in RFC6598 (https://tools.ietf.org/html/rfc6598)";
                if (addressIsMatch(getQuadInt(127, 0, 0, 0), 8, iIPAddress))
                    return "Reserved for Loopback\r\n(127.0.0.0 to 127.255.255.255) as described in RFC1122 (https://tools.ietf.org/html/rfc1122)";
                if (addressIsMatch(getQuadInt(169, 254, 0, 0), 16, iIPAddress))
                    return "Reserved for use as link-local autoconfig addresses\r\n(169.254.0.0 to 169.254.255.255) as described in RFCs 3927 (https://tools.ietf.org/html/rfc3927)";
                if (addressIsMatch(getQuadInt(172, 16, 0, 0), 12, iIPAddress))
                    return "Reserved for use in private networks only\r\n(172.16.0.0 to 172.31.255.255) as described in RFC1918 (https://tools.ietf.org/html/rfc1918)";
                if (addressIsMatch(getQuadInt(192, 0, 0, 0), 24, iIPAddress))
                {
                    string tempBind = "Reserved for IETF Protocol Assignments\r\n(192.0.0.0 to 192.0.0.255) as described in RFC6890 (https://tools.ietf.org/html/rfc6890)";
                    if (addressIsMatch(getQuadInt(192, 0, 0, 0), 24, iIPAddress))
                        tempBind = "Reserved for DS-Lite\r\n(192.0.0.0 to 192.0.0.7) as described in RFC6333 (https://tools.ietf.org/html/rfc6333) and \r\n" + tempBind;
                    return tempBind;
                }
                if (addressIsMatch(getQuadInt(192, 0, 2, 0), 24, iIPAddress))
                    return "Reserved for Documentation(TEST - NET - 1)\r\n(192.0.2.0 to 192.0.2.255) as described in RFC5737 (https://tools.ietf.org/html/rfc5737)";
                if (addressIsMatch(getQuadInt(192, 88, 99, 0), 24, iIPAddress))
                    return "Reserved for 6to4 Relay Anycast\r\n(192.88.99.0 to 192.88.99.255) as described in RFC2068 (https://tools.ietf.org/html/rfc3068)";
                if (addressIsMatch(getQuadInt(192, 168, 0, 0), 16, iIPAddress))
                    return "Reserved for use in private networks only\r\n(192.168.0.0 to 192.168.255.255) as described in RFC1918 (https://tools.ietf.org/html/rfc1918)";
                if (addressIsMatch(getQuadInt(198, 18, 0, 0), 15, iIPAddress))
                    return "Reserved for Benchmarking\r\n(198.18.0.0 to 198.19.255.255) as described in RFC2544 (https://tools.ietf.org/html/rfc2544)";
                if (addressIsMatch(getQuadInt(198, 51, 100, 0), 24, iIPAddress))
                    return "Reserved for Documentation (TEST-NET-2)\r\n(198.51.100.0 to 198.51.100.255) as described in RFC5737 (https://tools.ietf.org/html/rfc5737)";
                if (addressIsMatch(getQuadInt(203, 0, 113, 0), 24, iIPAddress))
                    return "Reserved for Documentation (TEST-NET-3)\r\n(203.0.113.0 to 203.0.113.255) as described in RFC5737 (https://tools.ietf.org/html/rfc5737)";
                if (addressIsMatch(getQuadInt(224, 0, 0, 0), 4, iIPAddress))
                    return "Reserved for Multicast\r\n(224.0.0.0 to 239.255.255.255) as described in RFC1112 (https://tools.ietf.org/html/rfc1112)";
                if (addressIsMatch(getQuadInt(255, 255, 255, 255), 32, iIPAddress)) // Must always be in front of 240.0.0.0/4
                    return "Reserved for Limited Broadcast\r\n(255.255.255.255) as described in RFC0919 (https://tools.ietf.org/html/rfc0919)";
                if (addressIsMatch(getQuadInt(240, 0, 0, 0), 4, iIPAddress))
                    return "Reserved for Future Use\r\n(240.0.0.0 to 255.255.255.254) as described in RFC1112 (https://tools.ietf.org/html/rfc1112)";
                    






                /*
                netAddress[0] = 255 & iIPAddress[0];
                netAddress[1] = 0 & iIPAddress[1];
                netAddress[2] = 0 & iIPAddress[2];
                netAddress[3] = 0 & iIPAddress[3];
                if (netAddress[0] == 10 && netAddress[1] == 0 && netAddress[2] == 0 && netAddress[3] == 0)
                    return "Reserved for use in private networks only\r\n(10.0.0.0 to 10.255.255.255) as described in RFC 1918";
                netAddress[0] = 255 & iIPAddress[0];
                netAddress[1] = 240 & iIPAddress[1];
                netAddress[2] = 0 & iIPAddress[2];
                netAddress[3] = 0 & iIPAddress[3];
                if (netAddress[0] == 172 && netAddress[1] == 16 && netAddress[2] == 0 && netAddress[3] == 0)
                    return "Reserved for use in private networks only\r\n(172.16.0.0 to 172.31.255.255) as described in RFC 1918";
                netAddress[0] = 255 & iIPAddress[0];
                netAddress[1] = 255 & iIPAddress[1];
                netAddress[2] = 0 & iIPAddress[2];
                netAddress[3] = 0 & iIPAddress[3];
                if (netAddress[0] == 192 && netAddress[1] == 168 && netAddress[2] == 0 && netAddress[3] == 0)
                    return "Reserved for use in private networks only\r\n(192.168.0.0 to 192.168.255.255) as described in RFC 1918";
                netAddress[0] = 255 & iIPAddress[0];
                netAddress[1] = 255 & iIPAddress[1];
                netAddress[2] = 255 & iIPAddress[2];
                netAddress[3] = 0 & iIPAddress[3];
                if (netAddress[0] == 169 && netAddress[1] == 254 && netAddress[2] > 0 && netAddress[2] < 255 && netAddress[3] == 0)
                    return "Reserved for use as link-local autoconfig addresses\r\n(169.254.1.0 to 169.254.254.255) as described in RFCs 3927 and 5735";
                */
            }
            return "";
        }

        private static int[] getQuadInt(int first, int second, int third, int fourth)
        {
            return new int[] { first, second, third, fourth };
        }

        private static bool addressIsMatch(int[] subnet, int maskBits, int[] address)
        {
            int n = -1; //n = 32Bit set to 1
            n <<= (32 - maskBits);
            int[] netmask = new int[4];
            for (int i = 3; i >= 0; i--)
            {
                netmask[i] = n & 255;
                n >>= 8;
            }
            return addressIsMatch(subnet, netmask, address);
        }

        private static bool addressIsMatch(int[] subnet, int[] netmask, int[] address)
        {
            if(subnet.Length != 4 || netmask.Length != 4 || address.Length != 4)
            {
                throw new ArgumentException("Arguments are not valid IP addresses!");
            }
            int[] test = new int[4];
            for(int i = 0; i < 4; i++)
            {
                if(subnet[i] < 0 || subnet[i] > 255 ||
                   netmask[i] < 0 || netmask[i] > 255 ||
                   address[i] < 0 || address[i] > 255)
                {
                    throw new ArgumentException("Arguments are not valid IP addresses!");
                }
                test[i] = netmask[i] & address[i];
            }
            if (test[0] == subnet[0] && test[1] == subnet[1] && test[2] == subnet[2] && test[3] == subnet[3])
                return true;
            return false;
        }

        private void txtDetails_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText.ToString());
        }
    }
}
