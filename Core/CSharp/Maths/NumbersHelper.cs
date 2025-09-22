using System;

namespace Core.Maths {
    public static class NumbersHelper {
        public static int? FromWord(string word) {
            word = word.ToLower();
            switch (word)
            {
                case "zero": return 0;
                case "one": return 1;
                case "single": return 1;
                case "two": return 2;
                case "double": return 2;
                case "three": return 3;
                case "tripple": return 3;
                case "four": return 4;
                case "five": return 5;
                case "six": return 6;
                case "seven": return 7;
                case "eight": return 8;
                case "nine": return 9;
                case "ten": return 10;
                case "eleven": return 11;
                case "twelve": return 12;
                case "thirteen": return 13;
                case "fourteen": return 14;
                case "fifteen": return 15;
                case "sixteen": return 16;
                case "seventeen": return 17;
                case "eighteen": return 18;
                case "nineteen": return 19;
                case "twenty": return 20;
                case "thirty": return 30;
                case "fourty": return 40;
                case "fifty": return 50;
                case "sixty": return 60;
                case "seventy": return 70;
                case "eighty": return 80 ;
                case "ninety": return 90;
                case "hundred": return 100;
                case "thousand": return 1000;
                case "grand": return 1000;
                case "million": return 1000000;
            }
            return null;
        }
    }
}