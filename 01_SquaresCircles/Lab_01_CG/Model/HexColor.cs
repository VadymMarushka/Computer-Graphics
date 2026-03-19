using CommunityToolkit.Mvvm.ComponentModel;
using OpenTK.Platform.Windows;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab_01_CG.Model
{
    public partial class HexColor : ObservableObject
    {
        public static readonly Dictionary<string, string> Colors = new Dictionary<string, string>
        {
            {"Black", "#000000" },
            {"Red", "#FF5555" },
            {"Green", "#55FF55" },
            {"Blue", "#5555FF" },
            {"Transparent" , "#00000000" },
        };
        public HexColor(string name)
        {
            Name = name;
        }
        public HexColor()
        {
            Name = "Black";
        }
        [ObservableProperty]
        private string _code;

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Code = Colors.GetValueOrDefault(_name) ?? "#000000";
            }
        }
    }
}
