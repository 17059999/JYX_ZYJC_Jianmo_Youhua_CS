using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class MyPublic_Api
    {
        public static double m_tolerance = 10e-7;
        public static bool is_double_xiangdeng(double d1, double d2)
        {
            
            if (Math.Abs(d1 - d2) < m_tolerance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);
        public static bool Is64Bit()
        {
            return true;
            //bool retVal;
            //IsWow64Process(Process.GetCurrentProcess().Handle, out retVal);
            //return retVal;
        }
        private const int xx = 20200409;
        private static string keyPath_64bit = @"SOFTWARE\Bentley\OpenPlantModeler";
        private static string shiyong_key = @"ZhongyeJingcheng_OPM_is_shiyong";
        private static string register_code_key = @"ZhongyeJingcheng_OPM_register_code";
        private static string installdate_key = @"ZhongyeJingcheng_OPM_installdate";
        public static bool is_register()
        {
            if (isRegister)
            {
                return isRegister;
            }
            else
            {
                Microsoft.Win32.RegistryKey mainKey = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey subKey;
                try
                {
                    if (Is64Bit())
                    {
                        subKey = mainKey.OpenSubKey(keyPath_64bit);
                    }
                    else
                    {
                        //subKey = mainKey.OpenSubKey(keyPath_32bit);
                        MessageBox.Show("仅适合安装于64位系统!");
                        return false;
                    }
                }
                catch
                {
                    MessageBox.Show("请先获取管理员权限!");
                    return false;
                }
                try
                {
                    string is_shiyong = subKey.GetValue(shiyong_key).ToString();
                    if (is_shiyong != null)
                    {
                        return true;
                    }
                    string register_code = subKey.GetValue(register_code_key).ToString();
                    if (register_code != null)
                    {
                        return true;
                    }

                }
                catch (Exception)
                {

                }
                return false;
            }
        }
        public static bool is_timeout()
        {
            Microsoft.Win32.RegistryKey mainKey = Microsoft.Win32.Registry.LocalMachine;
            Microsoft.Win32.RegistryKey subKey;
            try
            {
                if (Is64Bit())
                {
                    subKey = mainKey.OpenSubKey(keyPath_64bit);
                }
                else
                {
                    //subKey = mainKey.OpenSubKey(keyPath_32bit);
                    MessageBox.Show("仅适合安装于64位系统!");
                    return false;
                }

            }
            catch
            {
                MessageBox.Show("请先获取管理员权限!");
                return false;
            }

            try
            {
                string register_code = subKey.GetValue(register_code_key).ToString();
                if (register_code != null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {

            }

            string is_shiyong = subKey.GetValue(shiyong_key).ToString();
            if (is_shiyong.Equals("true"))
            {
                int time = Convert.ToInt32(Convert.ToDateTime(subKey.GetValue(installdate_key)).AddMonths(2).ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo));
                int now_time = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo));

                if (time >= now_time)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }


            return true;
        }
        public static string GetMacByIPConfig()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("ipconfig");
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            Process p = Process.Start(startInfo);
            p.WaitForExit();
            //截取输出流
            StreamReader reader = p.StandardOutput;
            string line = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    line = line.Trim();

                    if (line.StartsWith("Physical Address") || line.StartsWith("物理地址"))
                    {
                        string mac = line.Substring(line.Length - 17);
                        mac = mac.Replace("-", ":");
                        //等待程序执行完退出进程
                        p.WaitForExit();
                        p.Close();
                        reader.Close();

                        return mac;
                    }
                }

                line = reader.ReadLine();
            }

            //等待程序执行完退出进程
            p.WaitForExit();
            p.Close();
            reader.Close();

            return "";
        }

        public static string GetMac()
        {
            string mac = GetMacByIPConfig();
            if (mac == "")
            {
                NetworkInterface[] network_interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface network_interface in network_interfaces)
                {
                    mac = network_interface.GetPhysicalAddress().ToString();
                    mac = mac.Insert(10, ":");
                    mac = mac.Insert(8, ":");
                    mac = mac.Insert(6, ":");
                    mac = mac.Insert(4, ":");
                    mac = mac.Insert(2, ":");
                    return mac.Trim();
                }
            }
            return mac;
        }

        public static bool create_register(string str_value)
        {
            try
            {
                Microsoft.Win32.RegistryKey mainKey = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey subKey;
                if (Is64Bit())
                {
                    subKey = mainKey.OpenSubKey(keyPath_64bit, true);
                }
                else
                {
                    //subKey = mainKey.OpenSubKey(keyPath_32bit, true);

                    return false;
                }
                subKey.SetValue(register_code_key, str_value);
                subKey.SetValue(shiyong_key, "false");
                subKey.SetValue(installdate_key, DateTime.Now.ToString());
                isRegister = true;
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("请取得管理员权限!");
                return false;
            }
        }
        public static bool create_shiyong_register()
        {
            Microsoft.Win32.RegistryKey mainKey = Microsoft.Win32.Registry.LocalMachine;
            Microsoft.Win32.RegistryKey subKey;
            try
            {
                if (Is64Bit())
                {
                    subKey = mainKey.OpenSubKey(keyPath_64bit, true);
                }
                else
                {
                    //subKey = mainKey.OpenSubKey(keyPath_32bit, true);
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("请先获取管理员权限!");
                return false;
            }

            try
            {
                subKey.GetValue(shiyong_key).ToString();
            }
            catch (Exception)
            {
                try
                {
                    subKey.SetValue(shiyong_key, "true");

                    subKey.SetValue(installdate_key, DateTime.Now.ToString());
                    isRegister = true;
                    return true;
                }
                catch
                {
                    MessageBox.Show("请先获取管理员权限!");
                }
            }
            return false;
        }
        public static string jisuan_key(string mac)
        {
            char[] mac_chars = mac.ToCharArray();
            int mac_int = 0;
            foreach (char mac_char in mac_chars)
            {
                mac_int += (int)mac_char;
            }
            return Convert.ToString(mac_int * xx);
        }

        private static bool isRegister = false;
    }
}
