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
        // �ngilizce Erkek
        "John Smith", "Robert Brown", "Michael Miller", "William Moore", "David Anderson",
        "Richard Jackson", "Joseph Harris", "Thomas Thompson", "Charles Martinez", "Christopher Clark",

        // T�rk�e Erkek
        "Ahmet Y�lmaz", "Mehmet Demir", "Mustafa �elik", "Ali Y�ld�r�m", "H�seyin Arslan",
        "Hasan Karaca", "�brahim Polat", "�smail Yavuz", "Osman �im�ek", "Yusuf Kaplan",

        // Tayca Erkek
        "Somchai Sukhumvit", "Wira Thongchai", "Prayut Thammarat", "Chuchart Sukhothai", "Anan Ayutthaya",
        "Preecha Bangna", "Sombun Klongtoey", "Chaiwat Din Daeng", "Pongsak Samutprakarn", "Wittaya Huaikhwang",

        // �spanyolca Erkek
        "Carlos Garcia", "Jose Martinez", "Antonio Lopez", "Manuel Perez", "Francisco Ramirez",
        "Juan Flores", "Luis Diaz", "Javier Morales", "Miguel Cruz", "Pedro Castro",

        // �ince (Traditional) Erkek
        "Wang Wei", "Zhang Hua", "Chen Long", "Yang Lei", "Wu Jing",
        "Wang Lin", "Zhang Yu", "Chen Xi", "Yang Rui", "Wu Qiang",

        // �ince (Simplified) Erkek
        "Wang Jun", "Zhang Qiang", "Chen Jie", "Yang Ming", "Wu Dan",
        "Wang Yan", "Zhang Yu", "Chen Li", "Yang Lei", "Wu Jian",

        // K�rt�e Erkek
        "Azad Baran", "Ciya Roj", "Erdalan Hev�", "Firat Zana", "Hejar Welat",
        "Lorin Azadi", "Orhan Piroz", "Sipan Serxweb�n", "Teko�in Rojhat", "Zagros �iyar",

        // Rus�a Erkek
        "Aleksandr Ivanov", "Dmitriy Sidorov", "Sergey Kuznetsov", "Andrey Vasiliev", "Ivan Fedorov",
        "Mikhail Nikitin", "Nikolay Pavlov", "Yuriy Lebedev", "Aleksey Soloviev", "Vladimir Zaitsev",

        // Almanca Erkek
        "Hans M�ller", "Peter Schneider", "Thomas Weber", "Klaus Wagner", "Wolfgang Schulz",
        "J�rgen Bauer", "Werner Richter", "Helmut Wolf", "G�nter Schwarz", "Frank Braun",

        // Frans�zca Erkek
        "Jean Martin", "Michel Dubois", "Pierre Robert", "Claude Petit", "Alain Leroy",
        "Philippe Simon", "Jacques Lefevre", "Bernard Dupont", "Andr� Bonnet", "Daniel Girard",

        // Japonca Erkek
        "Takashi Tanaka", "Hiroshi Yamamoto", "Taro Kobayashi", "Kenji Kato", "Kazuya Hayashi",
        "Shigeru Mori", "Masahiro Kimura", "Kazuo Matsumoto", "Yoshio Ikeda", "Noboru Nishimura",

        // Korece Erkek
        "Kim Cheolsu", "Park Minsu", "Jung Junho", "Yoon Dohyun", "Jang Junhyuk",
        "Oh Jihoon", "Song Jihoon", "Baek Dusan", "Namgoong Min", "Cho Sungmin",

        // Arap�a Erkek
        "Ahmad Al-Farsi", "Muhammad Al-Rashid", "Ali Al-Nasser", "Hassan Al-Haj", "Yusuf Al-Tamimi",
        "Omar Al-Sadiq", "Khalid Al-Shami", "Ibrahim Al-Faraj", "Mahmoud Al-Masri", "Abdullah Al-Harthi"
    };

    public static List<string> NPCNamesFemale = new List<string>()
    {
        // �ngilizce Kad�n
        "Alice Johnson", "Mary Davis", "Jennifer Wilson", "Linda Taylor", "Elizabeth Thomas",
        "Susan White", "Margaret Martin", "Jessica Garcia", "Sarah Robinson", "Karen Rodriguez",

        // T�rk�e Kad�n
        "Ay�e Kaya", "Fatma �ahin", "Emine �z", "Zeynep Ayd�n", "Hatice Ko�",
        "Elif �ak�r", "Meryem Tekin", "Sultan Aksoy", "Aysel Kurt", "G�l Sar�",

        // Tayca Kad�n
        "Somying Charoen", "Mali Rattanakosin", "Wipa Chaiyaphum", "Prapa Pathumwan", "Jaruwan Laksi",
        "Patcharee Thonburi", "Kanchana Bangkapi", "Chitra Bangrak", "Nipapan Bangmod",

        // �spanyolca Kad�n
        "Maria Rodriguez", "Carmen Hernandez", "Ana Gonzalez", "Isabel Sanchez", "Dolores Torres",
        "Pilar Rivera", "Luisa Ortiz", "Teresa Fernandez", "Rosa Gutierrez", "Elena Ramos",

        // �ince (Traditional) Kad�n
        "Li Na", "Liu Fang", "Zhao Min", "Zhou Feng", "Zheng Ming",
        "Li Mei", "Liu Xin", "Zhao Yue", "Zhou Yi", "Zheng Hua",

        // �ince (Simplified) Kad�n
        "Li Jie", "Liu Yan", "Zhao Wei", "Zhou Li", "Zheng Hui",
        "Li Xin", "Liu Wei", "Zhao Xue", "Zhou Peng", "Zheng Feng",

        // K�rt�e Kad�n
        "Berfin �i�ek", "Dil�ad Botan", "Gulbahar Dilan", "Jiyan Z�lan", "Kurdistan Dilovan",
        "Medya �ore�", "Nalin Derya", "Perwin Serhed", "Rojin Newroz", "Viyan Asmin",

        // Rus�a Kad�n
        "Mariya Petrova", "Anna Smirnova", "Ekaterina Popova", "Natalya Mikhailova", "Olga Morozova",
        "Tatyana Zakharova", "Elena Novikova", "Lyudmila Kozlova", "Marina Vinogradova", "Galina Maximova",

        // Almanca Kad�n
        "Anna Schmidt", "Maria Fischer", "Ursula Meyer", "Monika Becker", "Ingrid Hoffmann",
        "Sabine Koch", "Renate Klein", "Petra Neumann", "Heike Zimmermann", "Susanne Hartmann",

        // Frans�zca Kad�n
        "Marie Bernard", "Nathalie Thomas", "Isabelle Richard", "Sophie Durand", "Martine Moreau",
        "Christine Laurent", "Catherine Fournier", "Val�rie Lambert", "Sandrine Fran�ois", "H�l�ne Roux",

        // Japonca Kad�n
        "Yuko Suzuki", "Keiko Nakamura", "Akiko Saito", "Yumi Yamada", "Naoko Shimizu",
        "Haruka Inoue", "Emi Sasaki", "Miyuki Shibata", "Reiko Okada", "Aiko Fukuda",

        // Korece Kad�n
        "Lee Younghee", "Choi Jiyoung", "Kang Suji", "Cho Sumin", "Im Haneul",
        "Shin Yuri", "Hong Gildong", "Hanra", "Seo Jisoo", "Ahn Hyojin",

        // Arap�a Kad�n
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
    "Hilma af Klint", "Sofonisba Anguissola", "K�the Kollwitz", "Tamara de Lempicka", "Judith Leyster",
    "Lee Krasner", "Marie Bracquemond", "Angelica Kauffman", "Elisabeth Vig�e Le Brun", "Gabriele M�nter",
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
