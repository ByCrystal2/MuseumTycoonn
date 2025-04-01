using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constant
{
    private const string iAPIDCompany = "com_sidragames_";
    public static string IAPIDCompany { get { return iAPIDCompany; } }
    private const string iAPIDGame = "regallegacy_";
    public static string IAPIDGame { get { return iAPIDGame; } }

    private const string MainFolderName = "SidraGames";
    private const string GameFolderName = "RegalLegacy";
    public static readonly string SaveLocation = $"{MainFolderName}/{GameFolderName}";

    public static List<string> NPCNamesMale = new List<string>()
    {
        // Ýngilizce Erkek
        "John Smith", "Robert Brown", "Michael Miller", "William Moore", "David Anderson",
        "Richard Jackson", "Joseph Harris", "Thomas Thompson", "Charles Martinez", "Christopher Clark",

        // Türkçe Erkek
        "Ahmet Yýlmaz", "Mehmet Demir", "Mustafa Çelik", "Ali Yýldýrým", "Hüseyin Arslan",
        "Hasan Karaca", "Ýbrahim Polat", "Ýsmail Yavuz", "Osman Þimþek", "Yusuf Kaplan",

        // Tayca Erkek
        "Somchai Sukhumvit", "Wira Thongchai", "Prayut Thammarat", "Chuchart Sukhothai", "Anan Ayutthaya",
        "Preecha Bangna", "Sombun Klongtoey", "Chaiwat Din Daeng", "Pongsak Samutprakarn", "Wittaya Huaikhwang",

        // Ýspanyolca Erkek
        "Carlos Garcia", "Jose Martinez", "Antonio Lopez", "Manuel Perez", "Francisco Ramirez",
        "Juan Flores", "Luis Diaz", "Javier Morales", "Miguel Cruz", "Pedro Castro",

        // Çince (Traditional) Erkek
        "Wang Wei", "Zhang Hua", "Chen Long", "Yang Lei", "Wu Jing",
        "Wang Lin", "Zhang Yu", "Chen Xi", "Yang Rui", "Wu Qiang",

        // Çince (Simplified) Erkek
        "Wang Jun", "Zhang Qiang", "Chen Jie", "Yang Ming", "Wu Dan",
        "Wang Yan", "Zhang Yu", "Chen Li", "Yang Lei", "Wu Jian",

        // Kürtçe Erkek
        "Azad Baran", "Ciya Roj", "Erdalan Hevî", "Firat Zana", "Hejar Welat",
        "Lorin Azadi", "Orhan Piroz", "Sipan Serxwebûn", "Tekoþin Rojhat", "Zagros Þiyar",

        // Rusça Erkek
        "Aleksandr Ivanov", "Dmitriy Sidorov", "Sergey Kuznetsov", "Andrey Vasiliev", "Ivan Fedorov",
        "Mikhail Nikitin", "Nikolay Pavlov", "Yuriy Lebedev", "Aleksey Soloviev", "Vladimir Zaitsev",

        // Almanca Erkek
        "Hans Müller", "Peter Schneider", "Thomas Weber", "Klaus Wagner", "Wolfgang Schulz",
        "Jürgen Bauer", "Werner Richter", "Helmut Wolf", "Günter Schwarz", "Frank Braun",

        // Fransýzca Erkek
        "Jean Martin", "Michel Dubois", "Pierre Robert", "Claude Petit", "Alain Leroy",
        "Philippe Simon", "Jacques Lefevre", "Bernard Dupont", "André Bonnet", "Daniel Girard",

        // Japonca Erkek
        "Takashi Tanaka", "Hiroshi Yamamoto", "Taro Kobayashi", "Kenji Kato", "Kazuya Hayashi",
        "Shigeru Mori", "Masahiro Kimura", "Kazuo Matsumoto", "Yoshio Ikeda", "Noboru Nishimura",

        // Korece Erkek
        "Kim Cheolsu", "Park Minsu", "Jung Junho", "Yoon Dohyun", "Jang Junhyuk",
        "Oh Jihoon", "Song Jihoon", "Baek Dusan", "Namgoong Min", "Cho Sungmin",

        // Arapça Erkek
        "Ahmad Al-Farsi", "Muhammad Al-Rashid", "Ali Al-Nasser", "Hassan Al-Haj", "Yusuf Al-Tamimi",
        "Omar Al-Sadiq", "Khalid Al-Shami", "Ibrahim Al-Faraj", "Mahmoud Al-Masri", "Abdullah Al-Harthi"
    };

    public static List<string> NPCNamesFemale = new List<string>()
    {
        // Ýngilizce Kadýn
        "Alice Johnson", "Mary Davis", "Jennifer Wilson", "Linda Taylor", "Elizabeth Thomas",
        "Susan White", "Margaret Martin", "Jessica Garcia", "Sarah Robinson", "Karen Rodriguez",

        // Türkçe Kadýn
        "Ayþe Kaya", "Fatma Þahin", "Emine Öz", "Zeynep Aydýn", "Hatice Koç",
        "Elif Çakýr", "Meryem Tekin", "Sultan Aksoy", "Aysel Kurt", "Gül Sarý",

        // Tayca Kadýn
        "Somying Charoen", "Mali Rattanakosin", "Wipa Chaiyaphum", "Prapa Pathumwan", "Jaruwan Laksi",
        "Patcharee Thonburi", "Kanchana Bangkapi", "Chitra Bangrak", "Nipapan Bangmod",

        // Ýspanyolca Kadýn
        "Maria Rodriguez", "Carmen Hernandez", "Ana Gonzalez", "Isabel Sanchez", "Dolores Torres",
        "Pilar Rivera", "Luisa Ortiz", "Teresa Fernandez", "Rosa Gutierrez", "Elena Ramos",

        // Çince (Traditional) Kadýn
        "Li Na", "Liu Fang", "Zhao Min", "Zhou Feng", "Zheng Ming",
        "Li Mei", "Liu Xin", "Zhao Yue", "Zhou Yi", "Zheng Hua",

        // Çince (Simplified) Kadýn
        "Li Jie", "Liu Yan", "Zhao Wei", "Zhou Li", "Zheng Hui",
        "Li Xin", "Liu Wei", "Zhao Xue", "Zhou Peng", "Zheng Feng",

        // Kürtçe Kadýn
        "Berfin Çiçek", "Dilþad Botan", "Gulbahar Dilan", "Jiyan Zîlan", "Kurdistan Dilovan",
        "Medya Þoreþ", "Nalin Derya", "Perwin Serhed", "Rojin Newroz", "Viyan Asmin",

        // Rusça Kadýn
        "Mariya Petrova", "Anna Smirnova", "Ekaterina Popova", "Natalya Mikhailova", "Olga Morozova",
        "Tatyana Zakharova", "Elena Novikova", "Lyudmila Kozlova", "Marina Vinogradova", "Galina Maximova",

        // Almanca Kadýn
        "Anna Schmidt", "Maria Fischer", "Ursula Meyer", "Monika Becker", "Ingrid Hoffmann",
        "Sabine Koch", "Renate Klein", "Petra Neumann", "Heike Zimmermann", "Susanne Hartmann",

        // Fransýzca Kadýn
        "Marie Bernard", "Nathalie Thomas", "Isabelle Richard", "Sophie Durand", "Martine Moreau",
        "Christine Laurent", "Catherine Fournier", "Valérie Lambert", "Sandrine François", "Hélène Roux",

        // Japonca Kadýn
        "Yuko Suzuki", "Keiko Nakamura", "Akiko Saito", "Yumi Yamada", "Naoko Shimizu",
        "Haruka Inoue", "Emi Sasaki", "Miyuki Shibata", "Reiko Okada", "Aiko Fukuda",

        // Korece Kadýn
        "Lee Younghee", "Choi Jiyoung", "Kang Suji", "Cho Sumin", "Im Haneul",
        "Shin Yuri", "Hong Gildong", "Hanra", "Seo Jisoo", "Ahn Hyojin",

        // Arapça Kadýn
        "Fatima Al-Shehri", "Aisha Al-Mansour", "Khadija Al-Haddad", "Zaynab Al-Dabbagh", "Maryam Al-Farsi",
        "Salma Al-Qadi", "Noura Al-Bassam", "Hind Al-Hamad", "Sarah Al-Mutairi", "Reem Al-Ali"
    };

    public static List<string> FamousPainterNames = new List<string>()
{
    // Erkek Ressamlar
    "Leonardo da Vinci", "Michelangelo Buonarroti", "Vincent van Gogh", "Pablo Picasso", "Claude Monet",
    "Rembrandt van Rijn", "Henri Matisse", "Salvador Dali", "Edgar Degas", "Paul Cezanne",
    "Edouard Manet", "Gustav Klimt", "Eugene Delacroix", "Jackson Pollock", "Wassily Kandinsky",
    "Joan Miro", "Francisco Goya", "Albrecht Durer", "Georges Seurat", "Pierre-Auguste Renoir",

    // Kadin Ressamlar
    "Frida Kahlo", "Georgia O'Keeffe", "Mary Cassatt", "Artemisia Gentileschi", "Berthe Morisot",
    "Hilma af Klint", "Sofonisba Anguissola", "Käthe Kollwitz", "Tamara de Lempicka", "Judith Leyster",
    "Lee Krasner", "Marie Bracquemond", "Angelica Kauffman", "Elisabeth Vigée Le Brun", "Gabriele Münter",
    "Joan Mitchell", "Paula Modersohn-Becker", "Louise Elisabeth Vigee Le Brun", "Helen Frankenthaler", "Suzanne Valadon"
};

    public static string GetNPCName(bool isMale)
    {
        if (isMale)
        {
            return NPCNamesMale[Random.Range(0, NPCNamesMale.Count)];
        }
        else
        {
            return NPCNamesFemale[Random.Range(0, NPCNamesFemale.Count)];
        }
    }

    public static List<string> GetRandomFamousPaintersWithDesiredCount(int _howMuch)
    {
        List<string> result = new List<string>();
        for (int i = 0; i < _howMuch; i++)
            result.Add(FamousPainterNames[Random.Range(0, FamousPainterNames.Count)]);
        return result;
    }
}
