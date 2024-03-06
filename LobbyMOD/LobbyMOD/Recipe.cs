using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Team17.Online.Multiplayer.Messaging;
using OrderController;
using Team17.Online;
using System.Collections;
using System.Reflection;

namespace LobbyMODS
{
    public class Recipe
    {
        public static Harmony HarmonyInstance { get; set; }
        public static void log(string mes) => MODEntry.LogInfo(mes);
        public static ConfigEntry<bool> enabled;
        public static ConfigEntry<bool> namesymplify;
        public static ConfigEntry<bool> displayhistory;
        //public static ConfigEntry<bool> displaymore;
        public static ConfigEntry<bool> predict;
        public static int startTime;
        public static bool resets = false;
        public static Dictionary<string, string> map = new Dictionary<string, string>();
        public static Dictionary<string, string> symplifyed = new Dictionary<string, string>();
        public static Dictionary<string, int> apperancecount = new Dictionary<string, int>();
        public static Dictionary<string, double> possibility = new Dictionary<string, double>();
        public static string[] recipes = new string[256];
        public static string s;
        public static int levelorders = 0;
        public static int allorders = 0;
        public static int noworders = 0;
        public static int deliveredorders = 0;
        public static int historyrecipecount = 0;
        public static bool changephrase = false;
        private static MyOnScreenDebugDisplayRecipe OnScreenDisplayRecipe;
        private static RecipeDisplay recipedisplay = null;

        public static void initial()
        {
            map.Add("BananaSmoothie", "香蕉奶昔");
            map.Add("MelonSmoothie", "西瓜奶昔");
            map.Add("StrawberrySmoothie", "草莓奶昔");
            map.Add("BananaPineappleSmoothie", "香蕉菠萝奶昔");
            map.Add("MegaSmoothie", "四拼奶昔");
            map.Add("ChickenTomatoKebob", "鸡肉番茄烤串");
            map.Add("ChickenMeatTomatoKebob", "鸡肉番茄牛肉烤串");
            map.Add("MeatMushroomPineappleKebob", "菠萝蘑菇牛肉烤串");
            map.Add("MushroomPineappleTomatoKebob", "菠萝蘑菇番茄烤串");
            map.Add("Pancake_Plain", "素煎饼");
            map.Add("Pancake_Chocolate", "巧克力煎饼");
            map.Add("StrawberryPancake", "草莓煎饼");
            map.Add("BlueberryPancake", "蓝莓煎饼");
            map.Add("BeefBurger", "素汉堡");
            map.Add("BeefBurgerCheese", "芝士汉堡");
            map.Add("HawaiianBurger", "菠萝牛肉汉堡");
            map.Add("BeefBurgerMax", "生菜番茄汉堡");
            map.Add("BeefBurgerWithGreensNCheese", "生菜芝士汉堡");
            map.Add("SteamedSpecial_Meat", "蒸肉");
            map.Add("SteamedSpecial_Fish", "蒸鱼");
            map.Add("SteamedSpecial_Prawns", "蒸虾");
            map.Add("SteamedSpecial_Carrot", "蒸胡萝卜");
            map.Add("Salad_Plain", "生菜沙拉");
            map.Add("Salad_Tomato", "生菜番茄沙拉");
            map.Add("Salad_Cucumber", "生菜番茄黄瓜沙拉");
            map.Add("Sushi_PlainFish", "生鱼片");
            map.Add("Sushi_PlainPrawn", "生虾片");
            map.Add("Sushi_Fish", "鱼寿司");
            map.Add("Sushi_Cucumber", "黄瓜寿司");
            map.Add("Sushi_All", "鱼黄瓜寿司");
            map.Add("Pasta_MeatOnly_New", "牛肉意面");
            map.Add("Pasta_TomatoOnly_New", "番茄意面");
            map.Add("Pasta_MushroomOnly_New", "蘑菇意面");
            map.Add("Pasta_Marinara_New", "鱼虾意面");
            map.Add("ChickenNuggetsAndChips_ChickenOnly", "炸鸡");
            map.Add("ChickenNuggetsAndChips_ChipsOnly", "炸薯条");
            map.Add("ChickenNuggetsAndChips_All", "炸鸡薯条");
            map.Add("Meat_Burrito", "肉卷饼");
            map.Add("Chicken_Burrito", "鸡肉卷饼");
            map.Add("Mushroom_Burrito", "蘑菇卷饼");
            map.Add("MargheritaPizza", "披萨");
            map.Add("PeperoniPizza", "香肠披萨");
            map.Add("ChickenPizza", "鸡肉披萨");
            map.Add("Pizza_Olives", "橄榄披萨");
            map.Add("Cake_Plain", "蜂蜜蛋糕");
            map.Add("Cake_Chocolate", "巧克力蛋糕");
            map.Add("Cake_Carrot", "胡萝卜蛋糕");
            map.Add("DLC09_HotChocolate", "热可可");
            map.Add("DLC09_HotChocolateCream", "奶油热可可");
            map.Add("DLC09_HotChocolateMallow", "棉花糖热可可");
            map.Add("DLC09_HotChocolateMallowCream", "奶油棉花糖热可可");
            map.Add("DLC09_ChristmasPudding", "甜点");
            map.Add("DLC09_ChristmasPuddingWithOrange", "橙味甜点");
            map.Add("DLC09_Pancake_Plain", "素煎饼");
            map.Add("DLC09_Pancake_Chocolate", "巧克力煎饼");
            map.Add("DLC09_Pancake_Strawberry", "草莓煎饼");
            map.Add("DLC10_HotPot_Meat", "肉火锅");
            map.Add("DLC10_HotPot_Prawn", "虾火锅");
            map.Add("DLC10_HotPot_Mixed", "肉虾火锅");
            map.Add("DLC10_HotPot_DoubleMeat", "双肉火锅");
            map.Add("DLC10_HotPot_DoublePrawn", "双虾火锅");
            map.Add("DLC13_FruitPlatter_OrangePeach", "黄桃橙子");
            map.Add("DLC13_FruitPlatter_GrapesPeach", "黄桃葡萄");
            map.Add("DLC13_FruitPlatter_OrangeGrapes", "橙子葡萄");
            map.Add("DLC13_FruitPlatter_OrangePeachGrapes", "三拼水果拼盘");
            map.Add("DLC13_MoonPie_Strawberry", "草莓月饼");
            map.Add("DLC13_MoonPie_Watermelon", "西瓜月饼");
            map.Add("DLC13_MoonPie_Chocolate", "巧克力月饼");
            map.Add("DLC13_MoonPie_ChocolateStrawberry", "巧克力草莓月饼");
            map.Add("DLC10_FruitPlatter_OrangePeach", "黄桃橙子");
            map.Add("DLC10_FruitPlatter_GrapesPeach", "黄桃葡萄");
            map.Add("DLC10_FruitPlatter_OrangeGrapes", "橙子葡萄");
            map.Add("DLC10_FruitPlatter_OrangePeachGrapes", "三拼水果拼盘");
            map.Add("DLC11_Hotdog_Mustard", "芥末酱热狗(黄)");
            map.Add("DLC11_Hotdog_Onions_Mustard", "芥末酱洋葱热狗(葱黄)");
            map.Add("DLC11_Hotdog_Ketchup", "番茄酱热狗(红)");
            map.Add("DLC11_Hotdog_Onions_Ketchup", "番茄酱洋葱热狗(葱红)");
            map.Add("DLC11_Hotdog_Ketchup_Mustard", "双酱热狗(红黄)");
            map.Add("FruitPlatter_OrangePeach", "黄桃橙子");
            map.Add("FruitPlatter_GrapesPeach", "黄桃葡萄");
            map.Add("FruitPlatter_OrangeGrapes", "橙子葡萄");
            map.Add("FruitPlatter_OrangePeachGrapes", "三拼水果拼盘");
            map.Add("HotPot_Meat", "肉火锅");
            map.Add("HotPot_Prawn", "虾火锅");
            map.Add("HotPot_Mixed", "肉虾火锅");
            map.Add("HotPot_DoubleMeat", "双肉火锅");
            map.Add("HotPot_DoublePrawn", "双虾火锅");
            map.Add("HotChocolate", "热可可");
            map.Add("HotChocolateCream", "奶油热可可");
            map.Add("HotChocolateMallow", "棉花糖热可可");
            map.Add("HotChocolateMallowCream", "奶油棉花糖热可可");
            map.Add("ChristmasPudding", "甜点");
            map.Add("ChristmasPuddingWithOrange", "橙味甜点");
            map.Add("OrangeSodaFloat_Vanilla", "橙子汽水香草冰淇淋(黄)");
            map.Add("OrangeSodaFloat_Chocolate", "橙子汽水可可冰淇淋(黄)");
            map.Add("RootBeerFloat_Vanilla", "根汁汽水香草冰淇淋(棕)");
            map.Add("RootBeerFloat_Chocolate", "根汁汽水可可冰淇淋(棕)");
            map.Add("Tomato_Cucumber_Onion", "番茄黄瓜沙拉");
            map.Add("Tomato_Corn_Onion", "番茄玉米沙拉");
            map.Add("Salad_Corn_Onion", "生菜玉米沙拉");
            map.Add("Salad_Cucumber_Onion", "生菜黄瓜沙拉");
            map.Add("Salad_Tomato_Onion", "生菜番茄沙拉");
            map.Add("Salad_Cucumber_Tomato_Onion", "生菜番茄黄瓜沙拉");
            map.Add("MD_Burger_Fries", "薯条肉汉堡");
            map.Add("MD_Burger_Fries_CheeseSticks", "薯条芝士肉汉堡");
            map.Add("MD_Burger_OnionRings", "洋葱肉汉堡");
            map.Add("MD_Burger_OnionRings_CheeseSticks", "洋葱芝士肉汉堡");
            map.Add("MD_C_Burger_OnionRings", "洋葱鸡汉堡");
            map.Add("MD_C_Burger_Fries_OnionRings", "洋葱薯条鸡汉堡");
            map.Add("MD_C_Burger_CheeseSticks", "芝士鸡汉堡");
            map.Add("MD_C_Burger_Fries_CheeseSticks", "芝士薯条鸡汉堡");
            map.Add("MD_Burger_Drink01", "红饮料肉汉堡");
            map.Add("MD_Burger_OnionRings_Drink01", "红饮料洋葱肉汉堡");
            map.Add("MD_Burger_Fries_Drink02", "绿饮料薯条肉汉堡");
            map.Add("MD_Burger_CheeseSticks_Drink03", "黄饮料芝士肉汉堡");
            map.Add("MD_C_Burger_CheeseSticks_Drink02", "绿饮料芝士鸡汉堡");
            map.Add("MD_C_Burger_Fries_Drink03", "黄饮料薯条鸡汉堡");
            map.Add("MD_C_Burger_OnionRings_Drink01", "红饮料洋葱鸡汉堡");
            map.Add("MD_C_Burger_Drink03", "黄饮料鸡汉堡");
            map.Add("Hotdog_Plain", "热狗(素)");
            map.Add("Hotdog_Onions", "热狗(葱)");
            map.Add("Hotdog_Mustard", "芥末酱热狗(黄)");
            map.Add("Hotdog_Onions_Mustard", "芥末酱洋葱热狗(葱黄)");
            map.Add("Hotdog_Ketchup", "番茄酱热狗(红)");
            map.Add("Hotdog_Onions_Ketchup", "番茄酱洋葱热狗(葱红)");
            map.Add("Hotdog_Ketchup_Mustard", "双酱热狗(红黄)");
            map.Add("Donut_Plain", "蜂蜜甜甜圈");
            map.Add("Donut_Raspberry", "树莓甜甜圈");
            map.Add("Donut_Chocolate", "巧克力甜甜圈");
            map.Add("ChickenPotatoCarrotRoast", "鸡肉烧烤");
            map.Add("ChickenPotatoCarrotBroccoliRoast", "鸡肉西兰花烧烤");
            map.Add("BeefPotatoCarrotRoast", "牛肉烧烤");
            map.Add("BeefPotatoCarrotBroccoliRoast", "牛肉西兰花烧烤");
            map.Add("FruitPie_Apple", "苹果水果派");
            map.Add("FruitPie_Blackberry", "黑莓水果派");
            map.Add("FruitPie_Cherry", "樱桃水果派");
            map.Add("FruitPie_AppleCherry", "苹果樱桃水果派");
            map.Add("FruitPie_AppleBlackberry", "苹果黑莓水果派");
            map.Add("OnionPotatoSoupLeek", "土豆韭葱汤");
            map.Add("OnionCarrotPotatoSoup", "土豆胡萝卜汤");
            map.Add("OnionBroccoliCheeseSoup", "西兰花芝士汤");
            map.Add("Breakfast_Bacon_Egg", "培根蛋");
            map.Add("Breakfast_Bacon_Egg_Sausage", "培根蛋肠");
            map.Add("Breakfast_Sausage_Beans", "肠豆");
            map.Add("Breakfast_Sausage_Beans_Egg", "肠豆蛋");
            map.Add("Breakfast_Sausage_Beans_Egg_Bacon", "肠豆培根蛋");
            map.Add("Smores_Plain", "烤棉花糖饼干");
            map.Add("Smores_Banana", "香蕉烤棉花糖饼干");
            map.Add("Smores_Strawberry", "草莓烤棉花糖饼干");
            map.Add("Smores_Chocolate", "巧克力烤棉花糖饼干");
            map.Add("Smores_Strawberry_Banana", "草莓香蕉烤棉花糖饼干");
        }

        public static void symplify()
        {
            symplifyed.Add("BananaSmoothie", "香蕉");
            symplifyed.Add("MelonSmoothie", "西瓜");
            symplifyed.Add("StrawberrySmoothie", "草莓");
            symplifyed.Add("BananaPineappleSmoothie", "香蕉菠萝");
            symplifyed.Add("MegaSmoothie", "四拼");
            symplifyed.Add("ChickenTomatoKebob", "番茄鸡");
            symplifyed.Add("ChickenMeatTomatoKebob", "鸡番肉");
            symplifyed.Add("MeatMushroomPineappleKebob", "菠蘑肉");
            symplifyed.Add("MushroomPineappleTomatoKebob", "菠蘑番");
            symplifyed.Add("Pancake_Plain", "煎饼-素");
            symplifyed.Add("Pancake_Chocolate", "煎饼-巧克力");
            symplifyed.Add("StrawberryPancake", "煎饼-草莓");
            symplifyed.Add("BlueberryPancake", "煎饼-蓝莓");
            symplifyed.Add("BeefBurger", "汉堡-素");
            symplifyed.Add("BeefBurgerCheese", "汉堡-芝士");
            symplifyed.Add("HawaiianBurger", "汉堡-菠萝肉");
            symplifyed.Add("BeefBurgerMax", "汉堡-生菜番茄");
            symplifyed.Add("BeefBurgerWithGreensNCheese", "汉堡-生菜芝士");
            symplifyed.Add("SteamedSpecial_Meat", "蒸肉");
            symplifyed.Add("SteamedSpecial_Fish", "蒸鱼");
            symplifyed.Add("SteamedSpecial_Prawns", "蒸虾");
            symplifyed.Add("SteamedSpecial_Carrot", "蒸胡萝卜");
            symplifyed.Add("Salad_Plain", "沙拉-生菜");
            symplifyed.Add("Salad_Tomato", "沙拉-生菜番茄");
            symplifyed.Add("Salad_Cucumber", "沙拉-生菜番茄黄瓜");
            symplifyed.Add("Sushi_PlainFish", "刺身-鱼");
            symplifyed.Add("Sushi_PlainPrawn", "刺身-虾");
            symplifyed.Add("Sushi_Fish", "寿司-鱼");
            symplifyed.Add("Sushi_Cucumber", "寿司-黄瓜");
            symplifyed.Add("Sushi_All", "寿司-鱼黄瓜");
            symplifyed.Add("Pasta_MeatOnly_New", "意面-牛肉");
            symplifyed.Add("Pasta_TomatoOnly_New", "意面-番茄");
            symplifyed.Add("Pasta_MushroomOnly_New", "意面-蘑菇");
            symplifyed.Add("Pasta_Marinara_New", "意面-鱼虾");
            symplifyed.Add("ChickenNuggetsAndChips_ChickenOnly", "炸鸡");
            symplifyed.Add("ChickenNuggetsAndChips_ChipsOnly", "炸薯条");
            symplifyed.Add("ChickenNuggetsAndChips_All", "炸鸡薯条");
            symplifyed.Add("Meat_Burrito", "卷饼-肉");
            symplifyed.Add("Chicken_Burrito", "卷饼-鸡肉");
            symplifyed.Add("Mushroom_Burrito", "卷饼-蘑菇");
            symplifyed.Add("MargheritaPizza", "披萨-素");
            symplifyed.Add("PeperoniPizza", "披萨-香肠");
            symplifyed.Add("ChickenPizza", "披萨-鸡肉");
            symplifyed.Add("Pizza_Olives", "披萨-橄榄");
            symplifyed.Add("Cake_Plain", "蛋糕-素");
            symplifyed.Add("Cake_Chocolate", "蛋糕-巧克力");
            symplifyed.Add("Cake_Carrot", "蛋糕-胡萝卜");
            symplifyed.Add("DLC09_HotChocolate", "可可-素");
            symplifyed.Add("DLC09_HotChocolateCream", "可可-奶油");
            symplifyed.Add("DLC09_HotChocolateMallow", "可可-棉花糖");
            symplifyed.Add("DLC09_HotChocolateMallowCream", "可可-奶油棉花糖");
            symplifyed.Add("DLC09_ChristmasPudding", "甜点");
            symplifyed.Add("DLC09_ChristmasPuddingWithOrange", "橙味甜点");
            symplifyed.Add("DLC09_Pancake_Plain", "煎饼-素");
            symplifyed.Add("DLC09_Pancake_Chocolate", "煎饼-巧克力");
            symplifyed.Add("DLC09_Pancake_Strawberry", "煎饼-草莓");
            symplifyed.Add("DLC10_HotPot_Meat", "火锅-肉");
            symplifyed.Add("DLC10_HotPot_Prawn", "火锅-虾");
            symplifyed.Add("DLC10_HotPot_Mixed", "火锅-肉虾");
            symplifyed.Add("DLC10_HotPot_DoubleMeat", "火锅-双肉");
            symplifyed.Add("DLC10_HotPot_DoublePrawn", "火锅-双虾");
            symplifyed.Add("DLC13_FruitPlatter_OrangePeach", "黄桃+橙子");
            symplifyed.Add("DLC13_FruitPlatter_GrapesPeach", "黄桃+葡萄");
            symplifyed.Add("DLC13_FruitPlatter_OrangeGrapes", "橙子+葡萄");
            symplifyed.Add("DLC13_FruitPlatter_OrangePeachGrapes", "三拼");
            symplifyed.Add("DLC13_MoonPie_Strawberry", "月饼-草莓");
            symplifyed.Add("DLC13_MoonPie_Watermelon", "月饼-西瓜");
            symplifyed.Add("DLC13_MoonPie_Chocolate", "月饼-巧克力");
            symplifyed.Add("DLC13_MoonPie_ChocolateStrawberry", "月饼-巧克力草莓");
            symplifyed.Add("DLC10_FruitPlatter_OrangePeach", "黄桃+橙子");
            symplifyed.Add("DLC10_FruitPlatter_GrapesPeach", "黄桃+葡萄");
            symplifyed.Add("DLC10_FruitPlatter_OrangeGrapes", "橙子+葡萄");
            symplifyed.Add("DLC10_FruitPlatter_OrangePeachGrapes", "三拼");
            symplifyed.Add("DLC11_Hotdog_Mustard", "热狗-黄");
            symplifyed.Add("DLC11_Hotdog_Onions_Mustard", "热狗-葱黄");
            symplifyed.Add("DLC11_Hotdog_Ketchup", "热狗-红");
            symplifyed.Add("DLC11_Hotdog_Onions_Ketchup", "热狗-葱红");
            symplifyed.Add("DLC11_Hotdog_Ketchup_Mustard", "热狗-双酱-");
            symplifyed.Add("FruitPlatter_OrangePeach", "黄桃+橙子");
            symplifyed.Add("FruitPlatter_GrapesPeach", "黄桃+葡萄");
            symplifyed.Add("FruitPlatter_OrangeGrapes", "橙子+葡萄");
            symplifyed.Add("FruitPlatter_OrangePeachGrapes", "三拼");
            symplifyed.Add("HotPot_Meat", "火锅-肉");
            symplifyed.Add("HotPot_Prawn", "火锅-虾");
            symplifyed.Add("HotPot_Mixed", "火锅-肉虾");
            symplifyed.Add("HotPot_DoubleMeat", "火锅-双肉");
            symplifyed.Add("HotPot_DoublePrawn", "火锅-双虾");
            symplifyed.Add("HotChocolate", "可可-素");
            symplifyed.Add("HotChocolateCream", "可可-奶油");
            symplifyed.Add("HotChocolateMallow", "可可-棉花糖");
            symplifyed.Add("HotChocolateMallowCream", "可可-奶油棉花糖");
            symplifyed.Add("ChristmasPudding", "甜点");
            symplifyed.Add("ChristmasPuddingWithOrange", "橙味甜点");
            symplifyed.Add("OrangeSodaFloat_Vanilla", "冰淇淋-香草+黄");
            symplifyed.Add("OrangeSodaFloat_Chocolate", "冰淇淋-可可+黄");
            symplifyed.Add("RootBeerFloat_Vanilla", "冰淇淋-香草+棕");
            symplifyed.Add("RootBeerFloat_Chocolate", "冰淇淋-可可+棕");
            symplifyed.Add("Tomato_Cucumber_Onion", "沙拉-番茄黄瓜");
            symplifyed.Add("Tomato_Corn_Onion", "沙拉-番茄玉米");
            symplifyed.Add("Salad_Corn_Onion", "沙拉-生菜玉米");
            symplifyed.Add("Salad_Cucumber_Onion", "沙拉-生菜黄瓜");
            symplifyed.Add("Salad_Tomato_Onion", "沙拉-生菜番茄");
            symplifyed.Add("Salad_Cucumber_Tomato_Onion", "沙拉-生菜番茄黄瓜");
            symplifyed.Add("MD_Burger_Fries", "肉+薯条");
            symplifyed.Add("MD_Burger_Fries_CheeseSticks", "肉+薯条+芝士");
            symplifyed.Add("MD_Burger_OnionRings", "肉+葱");
            symplifyed.Add("MD_Burger_OnionRings_CheeseSticks", "肉+洋葱+芝士");
            symplifyed.Add("MD_C_Burger_OnionRings", "鸡+葱");
            symplifyed.Add("MD_C_Burger_Fries_OnionRings", "鸡+葱+薯条");
            symplifyed.Add("MD_C_Burger_CheeseSticks", "鸡+芝士");
            symplifyed.Add("MD_C_Burger_Fries_CheeseSticks", "鸡+芝士+薯条");
            symplifyed.Add("MD_Burger_Drink01", "肉+红");
            symplifyed.Add("MD_Burger_OnionRings_Drink01", "肉+红+葱");
            symplifyed.Add("MD_Burger_Fries_Drink02", "肉+绿+薯条");
            symplifyed.Add("MD_Burger_CheeseSticks_Drink03", "肉+黄+芝士");
            symplifyed.Add("MD_C_Burger_CheeseSticks_Drink02", "鸡+绿+芝士");
            symplifyed.Add("MD_C_Burger_Fries_Drink03", "鸡+黄+薯条");
            symplifyed.Add("MD_C_Burger_OnionRings_Drink01", "鸡+红+葱");
            symplifyed.Add("MD_C_Burger_Drink03", "鸡+黄");
            symplifyed.Add("Hotdog_Plain", "肠-素");
            symplifyed.Add("Hotdog_Onions", "肠-葱");
            symplifyed.Add("Hotdog_Mustard", "肠-黄");
            symplifyed.Add("Hotdog_Onions_Mustard", "肠-葱黄");
            symplifyed.Add("Hotdog_Ketchup", "肠-红");
            symplifyed.Add("Hotdog_Onions_Ketchup", "肠-葱红");
            symplifyed.Add("Hotdog_Ketchup_Mustard", "肠-双酱");
            symplifyed.Add("Donut_Plain", "蛋糕-黄");
            symplifyed.Add("Donut_Raspberry", "蛋糕-红");
            symplifyed.Add("Donut_Chocolate", "蛋糕-黑");
            symplifyed.Add("ChickenPotatoCarrotRoast", "烧烤-鸡");
            symplifyed.Add("ChickenPotatoCarrotBroccoliRoast", "烧烤-鸡+西兰花");
            symplifyed.Add("BeefPotatoCarrotRoast", "烧烤-肉");
            symplifyed.Add("BeefPotatoCarrotBroccoliRoast", "烧烤-肉+西兰花");
            symplifyed.Add("FruitPie_Apple", "派-苹果");
            symplifyed.Add("FruitPie_Blackberry", "派-黑莓");
            symplifyed.Add("FruitPie_Cherry", "派-樱桃");
            symplifyed.Add("FruitPie_AppleCherry", "派-苹果樱桃");
            symplifyed.Add("FruitPie_AppleBlackberry", "派-苹果黑莓");
            symplifyed.Add("OnionPotatoSoupLeek", "汤-土豆韭葱");
            symplifyed.Add("OnionCarrotPotatoSoup", "汤-土豆胡萝卜)");
            symplifyed.Add("OnionBroccoliCheeseSoup", "汤-西兰花芝士)");
            symplifyed.Add("Breakfast_Bacon_Egg", "肉蛋");
            symplifyed.Add("Breakfast_Bacon_Egg_Sausage", "肉蛋肠");
            symplifyed.Add("Breakfast_Sausage_Beans", "肠豆");
            symplifyed.Add("Breakfast_Sausage_Beans_Egg", "肠豆蛋");
            symplifyed.Add("Breakfast_Sausage_Beans_Egg_Bacon", "肠豆肉蛋");
            symplifyed.Add("Smores_Plain", "饼干-素");
            symplifyed.Add("Smores_Banana", "饼干-香蕉");
            symplifyed.Add("Smores_Strawberry", "饼干-草莓");
            symplifyed.Add("Smores_Chocolate", "饼干-巧克力");
            symplifyed.Add("Smores_Strawberry_Banana", "饼干-草莓香蕉");
        }

        public static void Awake()
        {
            enabled = MODEntry.Instance.Config.Bind<bool>("03-菜单功能开关", "(03区域总开关)菜单显示功能", false);
            displayhistory = MODEntry.Instance.Config.Bind<bool>("03-菜单功能开关", "显示历史菜单", false);
            //displaymore = MODEntry.Instance.Config.Bind<bool>("03-菜单功能开关", "显示未来菜单", false);
            predict = MODEntry.Instance.Config.Bind<bool>("03-菜单功能开关", "预测未来菜单", false);
            namesymplify = MODEntry.Instance.Config.Bind<bool>("03-菜单功能开关", "简化显示菜单名称", false);
            initial();
            symplify();
            OnScreenDisplayRecipe = new MyOnScreenDebugDisplayRecipe();
            OnScreenDisplayRecipe.Awake();
            HarmonyInstance = Harmony.CreateAndPatchAll(MethodBase.GetCurrentMethod().DeclaringType);
            MODEntry.AllHarmony.Add(HarmonyInstance);
            MODEntry.AllHarmonyName.Add(MethodBase.GetCurrentMethod().DeclaringType.Name);
        }

        public static void Update()
        {
            OnScreenDisplayRecipe.Update();
            if (MODEntry.IsInParty)
            {
                if (displayhistory.Value)
                {
                    //displaymore.Value = false;
                    predict.Value = false;
                }
                else if (predict.Value)
                {
                    displayhistory.Value = false;
                    //displaymore.Value = false;
                }
                //else if (displaymore.Value)
                //{
                //    displayhistory.Value = false;
                //    predict.Value = false;
                //}
            }
            else
            {
                predict.Value = false;
                displayhistory.Value = false;
            }
        }

        public static void OnGUI() => OnScreenDisplayRecipe.OnGUI();

        [HarmonyPatch(typeof(ClientOrderControllerBase), "OnFoodDelivered")]
        [HarmonyPrefix]
        public static bool Prefix(ref ClientOrderControllerBase __instance, bool _success, OrderID _orderID)
        {
            if (!enabled.Value || (!displayhistory.Value && !predict.Value)) return true;
            if (__instance != null) MClientOrderControllerBase.OnFoodDelivered(__instance, _success, _orderID);
            return false;
        }

        //[HarmonyPatch(typeof(ServerOrderControllerBase), "AddNewOrder", new Type[] { })]
        //[HarmonyPrefix]
        //public static bool Prefix(ref ServerOrderControllerBase __instance)
        //{
        //    ServerCampaignFlowController flowController = GameObject.FindObjectOfType<ServerCampaignFlowController>();
        //    LevelConfigBase levelConfig = flowController.GetLevelConfig();
        //    if (levelConfig.name == "s_dynamic_stage_04_1P" || levelConfig.name == "s_dynamic_stage_04_2P" || levelConfig.name == "s_dynamic_stage_04_3P" || levelConfig.name == "s_dynamic_stage_04_4P") return true;
        //    if (!enabled.Value || !displaymore.Value) return true;
        //    if (__instance != null) MServerOrderControllerBase.AddNewOrderPatch(__instance);
        //    return false;
        //}

        [HarmonyPatch(typeof(ClientOrderControllerBase), "AddNewOrder")]
        [HarmonyPrefix]
        public static bool Prefix(ref ClientOrderControllerBase __instance, Serialisable _data)
        {
            if (!enabled.Value || (!displayhistory.Value && !predict.Value)) return true;
            if (__instance != null) MClientOrderControllerBase.AddNewOrder(__instance, _data);
            return false;
        }

        [HarmonyPatch(typeof(LoadingScreenFlow), "NextScene", MethodType.Getter)]
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (!enabled.Value) return true;
            if (recipedisplay != null)
            {
                RemoveRecipeDisplay();
                allorders = 0;
                noworders = 0;
                levelorders = 0;
                deliveredorders = 0;
                historyrecipecount = 0;
                changephrase = false;
                apperancecount.Clear();
                possibility.Clear();
            }
            return true;
        }

        [HarmonyPatch(typeof(ClientDynamicFlowController), "OnDynamicLevelMessage")]
        [HarmonyPrefix]
        public static bool Prefix(ref ClientDynamicFlowController __instance, IOnlineMultiplayerSessionUserId _sessionId, Serialisable _serialisable)
        {
            if (!enabled.Value || (!displayhistory.Value && !predict.Value)) return true;
            if (__instance != null) MClientDynamicFlowController.OnDynamicLevelMessage(__instance, _sessionId, _serialisable);
            return false;
        }

        //public class MServerOrderControllerBase
        //{
        //    public static RecipeList.Entry[] recipelist = new RecipeList.Entry[256];
        //    public static int cnt = 0;

        //    public static void reset()
        //    {
        //        cnt = 0;
        //        s = "";
        //    }

        //    public static void AddNewOrderPatch(ServerOrderControllerBase __instance)
        //    {
        //        RoundData.RoundInstanceData data = __instance.m_roundInstanceData as RoundData.RoundInstanceData;
        //        if (recipedisplay == null) AddRecipeDisplay();
        //        if (data.RecipeCount == 0)
        //        {
        //            reset();
        //        }

        //        if (cnt == 0)
        //        {
        //            for (int i = 0; i < 255; i++)
        //            {
        //                RecipeList.Entry[] nextRecipe = __instance.m_roundData.GetNextRecipe(__instance.m_roundInstanceData);
        //                recipelist[i] = nextRecipe[0];
        //            }
        //        }
        //        s = "";
        //        for (int i = cnt + 1; i <= cnt + 8; i++)
        //        {
        //            if (namesymplify.Value)
        //                s += (symplifyed.ContainsKey(recipelist[i].m_order.name) ? $"{i + 1}" + '.' + symplifyed[recipelist[i].m_order.name] : "") + '\n';
        //            else s += (map.ContainsKey(recipelist[i].m_order.name) ? $"{i + 1}" + '.' + map[recipelist[i].m_order.name] : "") + '\n';
        //        }
        //        recipedisplay.add_m_Text(s);
        //        __instance.AddNewOrder(recipelist[cnt++]);
        //    }
        //}

        public class MClientOrderControllerBase
        {
            public static void OnFoodDelivered(ClientOrderControllerBase __instance, bool _success, OrderID _orderID)
            {
                if (_success)
                {
                    ClientOrderControllerBase.ActiveOrder activeOrder = __instance.m_activeOrders.Find((ClientOrderControllerBase.ActiveOrder x) => x.ID == _orderID);
                    if (activeOrder != null)
                    {
                        __instance.m_gui.RemoveElement(activeOrder.UIToken, new RecipeSuccessAnimation());
                    }
                    __instance.m_activeOrders.RemoveAll((ClientOrderControllerBase.ActiveOrder x) => x.ID == _orderID);
                    if (displayhistory.Value)
                    {
                        if (noworders != 0 && deliveredorders == noworders)
                        {
                            historyrecipecount = 0;
                            deliveredorders = 0;
                            allorders -= noworders;
                            noworders = 0;
                        }
                        for (int i = Math.Max(0, 7 - historyrecipecount); i < 7; i++)
                            recipes[i] = recipes[i + 1];
                        recipes[7] = activeOrder.RecipeListEntry.m_order.name;
                        if (recipedisplay == null) AddRecipeDisplay();
                        s = "";
                        for (int i = 7; i >= Math.Max(7 - historyrecipecount, 0); i--)
                        {
                            if (namesymplify.Value)
                                s += (symplifyed.ContainsKey(recipes[i]) ? $"{8 - i}" + '.' + symplifyed[recipes[i]] : "") + '\n';
                            else s += (map.ContainsKey(recipes[i]) ? $"{8 - i}" + '.' + map[recipes[i]] : "") + '\n';
                        }
                        recipedisplay.add_m_Text(s);
                        historyrecipecount++;
                        deliveredorders++;
                    }
                }
                else
                {
                    for (int i = 0; i < __instance.m_activeOrders.Count; i++)
                    {
                        __instance.m_gui.PlayAnimationOnElement(__instance.m_activeOrders[i].UIToken, new RecipeFailureAnimation());
                    }
                }
            }

            public static void AddNewOrder(ClientOrderControllerBase __instance, Serialisable _data)
            {
                ServerOrderData data = (ServerOrderData)_data;
                RecipeList.Entry entry = new RecipeList.Entry();
                entry.Copy(data.RecipeListEntry);
                RecipeFlowGUI.ElementToken token = __instance.m_gui.AddElement(entry.m_order, data.Lifetime, __instance.m_expiredDoNothingCallback);
                ClientOrderControllerBase.ActiveOrder item = new ClientOrderControllerBase.ActiveOrder(data.ID, entry, token);
                __instance.m_activeOrders.Add(item);
                if (displayhistory.Value)
                {
                    if (changephrase)
                    {
                        noworders = allorders;
                        changephrase = false;
                    }
                }
                else if (predict.Value)
                {
                    if (changephrase)
                    {
                        allorders = 0;
                        changephrase = false;
                        apperancecount.Clear();
                        possibility.Clear();
                    }
                }
                allorders++;
                if (predict.Value)
                {
                    if (apperancecount.Count == 0)
                    {
                        ClientCampaignFlowController flowController = GameObject.FindObjectOfType<ClientCampaignFlowController>();
                        LevelConfigBase levelConfig = flowController.GetLevelConfig();
                        for (int i = 0; i < levelConfig.GetAllRecipes().Count; i++)
                        {
                            if (!apperancecount.ContainsKey(levelConfig.GetAllRecipes()[i].name))
                            {
                                apperancecount.Add(levelConfig.GetAllRecipes()[i].name, 0);
                            }
                            if (!possibility.ContainsKey(levelConfig.GetAllRecipes()[i].name))
                            {
                                possibility.Add(levelConfig.GetAllRecipes()[i].name, 0);
                            }
                        }
                        levelorders = apperancecount.Count;
                    }
                    apperancecount[data.RecipeListEntry.m_order.name]++;
                    double num = 0;
                    foreach (string key in apperancecount.Keys)
                    {
                        double cal = ((double)(allorders + 2) / (double)levelorders) - apperancecount[key];
                        possibility[key] = cal >= 0 ? cal : 0;
                        num += possibility[key];
                    }
                    var sortResult = from pair in possibility orderby pair.Value descending select pair;
                    if (num != 0)
                    {
                        int cnt = 0;
                        if (recipedisplay == null) AddRecipeDisplay();
                        s = "";
                        foreach (KeyValuePair<string, double> res in sortResult)
                        {
                            if (cnt++ == 7) break;
                            if (namesymplify.Value)
                                s += (symplifyed.ContainsKey(res.Key) ? $"{cnt}" + '.' + symplifyed[res.Key] + " " + (int)(res.Value * 100 / num + 0.5) + "%" : "") + '\n';
                            else s += (map.ContainsKey(res.Key) ? $"{cnt}" + '.' + map[res.Key] + " " + (int)(res.Value * 100 / num + 0.5) + "%" : "") + '\n';

                        }
                        recipedisplay.add_m_Text(s);
                    }
                }

            }
        }

        public class MClientDynamicFlowController
        {
            public static void OnDynamicLevelMessage(ClientDynamicFlowController __instance, IOnlineMultiplayerSessionUserId _sessionId, Serialisable _serialisable)
            {
                DynamicLevelMessage dynamicLevelMessage = (DynamicLevelMessage)_serialisable;
                IEnumerator item = __instance.BuildTransitionToPhaseRoutine(dynamicLevelMessage.m_phase);
                __instance.m_phaseQueue.Enqueue(item);
                changephrase = true;
            }
        }

        private static void AddRecipeDisplay()
        {
            recipedisplay = new RecipeDisplay();
            OnScreenDisplayRecipe.AddDisplay(recipedisplay);
            recipedisplay.init_m_Text();
        }

        private static void RemoveRecipeDisplay()
        {
            OnScreenDisplayRecipe.RemoveDisplay(recipedisplay);
            recipedisplay.OnDestroy();
            recipedisplay = null;
        }

        public class RecipeDisplay : DebugDisplay
        {

            public void init_m_Text() => m_Text = "";

            public void add_m_Text(string str)
            {
                m_Text = str;
            }

            public override void OnSetUp() { }

            public override void OnUpdate() { }

            public override void OnDraw(ref Rect rect, GUIStyle style) => base.DrawText(ref rect, style, m_Text);

            private static string m_Text = string.Empty;
        }

        private class MyOnScreenDebugDisplayRecipe
        {
            private readonly List<DebugDisplay> m_Displays = new List<DebugDisplay>();
            private readonly GUIStyle m_GUIStyle = new GUIStyle();

            public void AddDisplay(DebugDisplay display)
            {
                m_GUIStyle.fontSize = MODEntry.defaultFontSize.Value;
                try
                {
                    this.m_GUIStyle.normal.textColor = HexToColor(MODEntry.defaultFontColor.Value);
                }
                catch
                {
                    this.m_GUIStyle.normal.textColor = HexToColor("#FFFFFF");
                }
                if (display != null)
                {
                    display.OnSetUp();
                    m_Displays.Add(display);
                }
            }

            public void RemoveDisplay(DebugDisplay display) => m_Displays.Remove(display);

            public void Awake()
            {
                m_GUIStyle.alignment = TextAnchor.UpperLeft;
                //m_GUIStyle.fontStyle = FontStyle.Bold;

            }

            public void Update()
            {
                for (int i = 0; i < m_Displays.Count; i++)
                    m_Displays[i].OnUpdate();
            }

            public void OnGUI()
            {
                m_GUIStyle.fontSize = MODEntry.defaultFontSize.Value;
                try
                {
                    this.m_GUIStyle.normal.textColor = HexToColor(MODEntry.defaultFontColor.Value);
                }
                catch
                {
                    this.m_GUIStyle.normal.textColor = HexToColor("#FFFFFF");
                }
                Rect rect = new Rect(20f, 350f, Screen.width, m_GUIStyle.fontSize);
                for (int i = 0; i < m_Displays.Count; i++)
                    m_Displays[i].OnDraw(ref rect, m_GUIStyle);
            }

            private static Color HexToColor(string hex)
            {
                Color color = new Color();
                ColorUtility.TryParseHtmlString(hex, out color);
                return color;
            }
        }
    }
}
