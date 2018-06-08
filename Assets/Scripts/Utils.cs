using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tangram{

	public enum Level_Names {BOAT, CAT, CHAIR, DOG, FISH, HORSE, HOUSE, MOUNTAINS, RABBIT, SQUARE, SWAN};
    public enum Difficulty_Levels {EASY=1, MEDIUM, HARD};
	public enum Piece_States {IDLE, PICKED, LOCKED, JUST_MOVED};
	public enum Return_Values {CLEAN, ERROR, INCOMPLETE, IMPOSSIBLE};

    public static class Util_Methods {

        public static string level_val_to_name(int level) {
           return ((Level_Names)level).ToString().ToLower();
        }

        public static int level_name_to_val(string level_name) {
            return (int)Enum.Parse(typeof(Level_Names), level_name.ToUpper());
        }

        public static string difficulty_val_to_name(int difficulty) {
            return ((Difficulty_Levels)difficulty).ToString().ToLower();
        }

        public static int difficulty_name_to_val(string difficulty_name) {
            return (int)Enum.Parse(typeof(Difficulty_Levels), difficulty_name.ToUpper());
        }

    }

	public static class Constants {

        public const string LOG_PATH = "Logs/";

        public static readonly Dictionary<string, List<Vector3>> PTRS_LEVEL_SQUARE = new Dictionary<string, List<Vector3>> { {"little_triangle_red", new List<Vector3> {new Vector3 (17.02f, 2.92f, 0.0f), new Vector3 (0, 0, -90)}},
                                                                                                                        {"little_triangle_purple", new List<Vector3> {new Vector3 (13.38f, 7.66f, 0.0f), new Vector3 (0, 0, -180)}},
                                                                                                                        {"big_triangle_blue", new List<Vector3> {new Vector3 (-13.65f, 7.69f, 0.0f), new Vector3 (0, 0, 0)}},
                                                                                                                        {"big_triangle_orange", new List<Vector3> {new Vector3 (-11.54f, -5.12f, 0.0f), new Vector3 (0, 0, 90)}},
                                                                                                                        {"medium_triangle_pink", new List<Vector3> {new Vector3 (14.38f, -5.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"square_green", new List<Vector3> {new Vector3 (10.58f, 1.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"trapezoid_yellow", new List<Vector3> {new Vector3 (-17.29f, -1.92f, 0.0f), new Vector3 (0, 0, -90)}}};

        public static readonly Dictionary<string, List<Vector3>> PTRS_LEVEL_BOAT = new Dictionary<string, List<Vector3>> { {"little_triangle_red", new List<Vector3> {new Vector3 (17.52f, 2.92f, 0.0f), new Vector3 (0, 0, -90)}},
                                                                                                                        {"little_triangle_purple", new List<Vector3> {new Vector3 (13.38f, 7.66f, 0.0f), new Vector3 (0, 0, -180)}},
                                                                                                                        {"big_triangle_blue", new List<Vector3> {new Vector3 (-13.65f, 7.69f, 0.0f), new Vector3 (0, 0, 0)}},
                                                                                                                        {"big_triangle_orange", new List<Vector3> {new Vector3 (-11.54f, -5.12f, 0.0f), new Vector3 (0, 0, 90)}},
                                                                                                                        {"medium_triangle_pink", new List<Vector3> {new Vector3 (14.38f, -5.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"square_green", new List<Vector3> {new Vector3 (10.58f, 1.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"trapezoid_yellow", new List<Vector3> {new Vector3 (-17.29f, -1.92f, 0.0f), new Vector3 (0, 0, -90)}}};

        public static readonly Dictionary<string, List<Vector3>> PTRS_LEVEL_CAT = new Dictionary<string, List<Vector3>> { {"little_triangle_red", new List<Vector3> {new Vector3 (17.52f, 2.92f, 0.0f), new Vector3 (0, 0, -90)}},
                                                                                                                        {"little_triangle_purple", new List<Vector3> {new Vector3 (13.38f, 7.66f, 0.0f), new Vector3 (0, 0, -180)}},
                                                                                                                        {"big_triangle_blue", new List<Vector3> {new Vector3 (-13.65f, 7.69f, 0.0f), new Vector3 (0, 0, 0)}},
                                                                                                                        {"big_triangle_orange", new List<Vector3> {new Vector3 (-11.54f, -5.12f, 0.0f), new Vector3 (0, 0, 90)}},
                                                                                                                        {"medium_triangle_pink", new List<Vector3> {new Vector3 (14.38f, -5.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"square_green", new List<Vector3> {new Vector3 (10.58f, 1.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"trapezoid_yellow", new List<Vector3> {new Vector3 (-17.29f, -1.92f, 0.0f), new Vector3 (0, 0, -90)}}};

        public static readonly Dictionary<string, List<Vector3>> PTRS_LEVEL_CHAIR = new Dictionary<string, List<Vector3>> { {"little_triangle_red", new List<Vector3> {new Vector3 (17.52f, 2.92f, 0.0f), new Vector3 (0, 0, -90)}},
                                                                                                                        {"little_triangle_purple", new List<Vector3> {new Vector3 (13.38f, 7.66f, 0.0f), new Vector3 (0, 0, -180)}},
                                                                                                                        {"big_triangle_blue", new List<Vector3> {new Vector3 (-13.65f, 7.69f, 0.0f), new Vector3 (0, 0, 0)}},
                                                                                                                        {"big_triangle_orange", new List<Vector3> {new Vector3 (-11.54f, -5.12f, 0.0f), new Vector3 (0, 0, 90)}},
                                                                                                                        {"medium_triangle_pink", new List<Vector3> {new Vector3 (14.38f, -5.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"square_green", new List<Vector3> {new Vector3 (10.58f, 1.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"trapezoid_yellow", new List<Vector3> {new Vector3 (-17.29f, -1.92f, 0.0f), new Vector3 (0, 0, -90)}}};

        public static readonly Dictionary<string, List<Vector3>> PTRS_LEVEL_DOG = new Dictionary<string, List<Vector3>> { {"little_triangle_red", new List<Vector3> {new Vector3 (17.52f, 2.92f, 0.0f), new Vector3 (0, 0, -90)}},
                                                                                                                        {"little_triangle_purple", new List<Vector3> {new Vector3 (13.38f, 7.66f, 0.0f), new Vector3 (0, 0, -180)}},
                                                                                                                        {"big_triangle_blue", new List<Vector3> {new Vector3 (-13.65f, 7.69f, 0.0f), new Vector3 (0, 0, 0)}},
                                                                                                                        {"big_triangle_orange", new List<Vector3> {new Vector3 (-11.54f, -5.12f, 0.0f), new Vector3 (0, 0, 90)}},
                                                                                                                        {"medium_triangle_pink", new List<Vector3> {new Vector3 (14.38f, -5.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"square_green", new List<Vector3> {new Vector3 (10.58f, 1.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"trapezoid_yellow", new List<Vector3> {new Vector3 (-17.29f, -1.92f, 0.0f), new Vector3 (0, 0, -90)}}};

        public static readonly Dictionary<string, List<Vector3>> PTRS_LEVEL_FISH = new Dictionary<string, List<Vector3>> { {"little_triangle_red", new List<Vector3> {new Vector3 (17.52f, 2.92f, 0.0f), new Vector3 (0, 0, -90)}},
                                                                                                                        {"little_triangle_purple", new List<Vector3> {new Vector3 (13.38f, 7.66f, 0.0f), new Vector3 (0, 0, -180)}},
                                                                                                                        {"big_triangle_blue", new List<Vector3> {new Vector3 (-13.65f, 7.69f, 0.0f), new Vector3 (0, 0, 0)}},
                                                                                                                        {"big_triangle_orange", new List<Vector3> {new Vector3 (-11.54f, -5.12f, 0.0f), new Vector3 (0, 0, 90)}},
                                                                                                                        {"medium_triangle_pink", new List<Vector3> {new Vector3 (14.38f, -5.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"square_green", new List<Vector3> {new Vector3 (10.58f, 1.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"trapezoid_yellow", new List<Vector3> {new Vector3 (-17.29f, -1.92f, 0.0f), new Vector3 (0, 0, -90)}}};

        public static readonly Dictionary<string, List<Vector3>> PTRS_LEVEL_HORSE = new Dictionary<string, List<Vector3>> { {"little_triangle_red", new List<Vector3> {new Vector3 (17.52f, 2.92f, 0.0f), new Vector3 (0, 0, -90)}},
                                                                                                                        {"little_triangle_purple", new List<Vector3> {new Vector3 (13.38f, 7.66f, 0.0f), new Vector3 (0, 0, -180)}},
                                                                                                                        {"big_triangle_blue", new List<Vector3> {new Vector3 (-13.65f, 7.69f, 0.0f), new Vector3 (0, 0, 0)}},
                                                                                                                        {"big_triangle_orange", new List<Vector3> {new Vector3 (-11.54f, -5.12f, 0.0f), new Vector3 (0, 0, 90)}},
                                                                                                                        {"medium_triangle_pink", new List<Vector3> {new Vector3 (14.38f, -5.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"square_green", new List<Vector3> {new Vector3 (10.58f, 1.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"trapezoid_yellow", new List<Vector3> {new Vector3 (-17.29f, -1.92f, 0.0f), new Vector3 (0, 0, -90)}}};

        public static readonly Dictionary<string, List<Vector3>> PTRS_LEVEL_HOUSE = new Dictionary<string, List<Vector3>> { {"little_triangle_red", new List<Vector3> {new Vector3 (17.52f, 2.92f, 0.0f), new Vector3 (0, 0, -90)}},
                                                                                                                        {"little_triangle_purple", new List<Vector3> {new Vector3 (13.38f, 7.66f, 0.0f), new Vector3 (0, 0, -180)}},
                                                                                                                        {"big_triangle_blue", new List<Vector3> {new Vector3 (-13.65f, 7.69f, 0.0f), new Vector3 (0, 0, 0)}},
                                                                                                                        {"big_triangle_orange", new List<Vector3> {new Vector3 (-11.54f, -5.12f, 0.0f), new Vector3 (0, 0, 90)}},
                                                                                                                        {"medium_triangle_pink", new List<Vector3> {new Vector3 (14.38f, -5.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"square_green", new List<Vector3> {new Vector3 (10.58f, 1.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"trapezoid_yellow", new List<Vector3> {new Vector3 (-17.29f, -1.92f, 0.0f), new Vector3 (0, 0, -90)}}};

        public static readonly Dictionary<string, List<Vector3>> PTRS_LEVEL_MOUNTAINS = new Dictionary<string, List<Vector3>> { {"little_triangle_red", new List<Vector3> {new Vector3 (17.52f, 2.92f, 0.0f), new Vector3 (0, 0, -90)}},
                                                                                                                        {"little_triangle_purple", new List<Vector3> {new Vector3 (13.38f, 7.66f, 0.0f), new Vector3 (0, 0, -180)}},
                                                                                                                        {"big_triangle_blue", new List<Vector3> {new Vector3 (-13.65f, 7.69f, 0.0f), new Vector3 (0, 0, 0)}},
                                                                                                                        {"big_triangle_orange", new List<Vector3> {new Vector3 (-11.54f, -5.12f, 0.0f), new Vector3 (0, 0, 90)}},
                                                                                                                        {"medium_triangle_pink", new List<Vector3> {new Vector3 (14.38f, -5.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"square_green", new List<Vector3> {new Vector3 (10.58f, 1.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"trapezoid_yellow", new List<Vector3> {new Vector3 (-17.29f, -1.92f, 0.0f), new Vector3 (0, 0, -90)}}};

        public static readonly Dictionary<string, List<Vector3>> PTRS_LEVEL_RABIT = new Dictionary<string, List<Vector3>> { {"little_triangle_red", new List<Vector3> {new Vector3 (17.52f, 2.92f, 0.0f), new Vector3 (0, 0, -90)}},
                                                                                                                        {"little_triangle_purple", new List<Vector3> {new Vector3 (13.38f, 7.66f, 0.0f), new Vector3 (0, 0, -180)}},
                                                                                                                        {"big_triangle_blue", new List<Vector3> {new Vector3 (-13.65f, 7.69f, 0.0f), new Vector3 (0, 0, 0)}},
                                                                                                                        {"big_triangle_orange", new List<Vector3> {new Vector3 (-11.54f, -5.12f, 0.0f), new Vector3 (0, 0, 90)}},
                                                                                                                        {"medium_triangle_pink", new List<Vector3> {new Vector3 (14.38f, -5.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"square_green", new List<Vector3> {new Vector3 (10.58f, 1.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"trapezoid_yellow", new List<Vector3> {new Vector3 (-17.29f, -1.92f, 0.0f), new Vector3 (0, 0, -90)}}};

        public static readonly Dictionary<string, List<Vector3>> PTRS_LEVEL_SWAN = new Dictionary<string, List<Vector3>> { {"little_triangle_red", new List<Vector3> {new Vector3 (17.52f, 2.92f, 0.0f), new Vector3 (0, 0, -90)}},
                                                                                                                        {"little_triangle_purple", new List<Vector3> {new Vector3 (13.38f, 7.66f, 0.0f), new Vector3 (0, 0, -180)}},
                                                                                                                        {"big_triangle_blue", new List<Vector3> {new Vector3 (-13.65f, 7.69f, 0.0f), new Vector3 (0, 0, 0)}},
                                                                                                                        {"big_triangle_orange", new List<Vector3> {new Vector3 (-11.54f, -5.12f, 0.0f), new Vector3 (0, 0, 90)}},
                                                                                                                        {"medium_triangle_pink", new List<Vector3> {new Vector3 (14.38f, -5.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"square_green", new List<Vector3> {new Vector3 (10.58f, 1.47f, 0.0f), new Vector3 (0, 0, 45)}},
                                                                                                                        {"trapezoid_yellow", new List<Vector3> {new Vector3 (-17.29f, -1.92f, 0.0f), new Vector3 (0, 0, -90)}}};

        public static readonly Dictionary<int, Dictionary<string, List<Vector3>>> PTRS_POS =
            new Dictionary<int, Dictionary<string, List<Vector3>>> { { (int)Level_Names.CAT, PTRS_LEVEL_CAT }, { (int)Level_Names.BOAT, PTRS_LEVEL_BOAT }, { (int)Level_Names.CHAIR, PTRS_LEVEL_CHAIR },
                {(int)Level_Names.DOG, PTRS_LEVEL_DOG}, {(int)Level_Names.FISH, PTRS_LEVEL_FISH}, {(int)Level_Names.HORSE, PTRS_LEVEL_HORSE}, {(int)Level_Names.HOUSE, PTRS_LEVEL_HOUSE},
                { (int)Level_Names.MOUNTAINS, PTRS_LEVEL_MOUNTAINS}, {(int)Level_Names.RABBIT, PTRS_LEVEL_RABIT}, {(int)Level_Names.SQUARE, PTRS_LEVEL_SQUARE}, {(int)Level_Names.SWAN, PTRS_LEVEL_SWAN} };

    }
}

