using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Playables;
using I2.Loc;
using System.Linq;

public class TutoSceneTimeLine : MonoBehaviour
{
    private PlayableDirector currentActiveCutscene;
    public VideoPlayer videoPlayer;
    public Text SubtitleText;
    public CanvasGroup SubtitleCanvas;
    public List<GameObject> CloseEndVideo;

    private SubtitleData subtitleData;
    List<SubtitleData> subtitleDatas = new List<SubtitleData>();
    private void Awake()
    {
        if (AudioManager.instance != null)
         AudioManager.instance.PlayMusicOfTutorial();        
    }
    void Start()
    {
        InitSecondsAndTexts();
        subtitleData = GetCurrentLanguageSubtitleData();

        videoPlayer.playOnAwake = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.Play();

        //currentActiveCutscene = gameObject.GetComponent<PlayableDirector>();
        //currentActiveCutscene.stopped += OnPlayableDirectorStopped;
    }
    public void InitSecondsAndTexts()
    {
        SubtitleData EnData = new SubtitleData()
        {
            TargetLanguage = "English",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "Welcome to a Kingdom where time stands",
                "still, a land of splendour and",
                "power lost in the pages of history.",
                "This is the Kingdom of the Golden Museum.",
                "And here we are in the heart of the",
                "Kingdom, in the bustling streets,",
                "the clatter of horse-drawn carriages, the",
                "shouts of market vendors, and the",
                "footsteps of royal guards echo",
                "through.",
                "you are here to take on the task of",
                "preserving the kingdom's rich history and",
                "cultural heritage.",
                "The King entrusts you with managing the",
                "Kingdom's greatest museum. This",
                "museum will house the past,",
                "present and future of the Kingdom.",
                "Welcome. Are you ready to write the",
                "story of the Kingdom together?"
            }
        };
        SubtitleData TrData = new SubtitleData()
        {
            TargetLanguage = "Turkish",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "Zamanın durduğu bir Krallığa hoş geldiniz",
                "tarih sayfalarında kaybolmuş",
                "görkem ve güç diyarına.",
                "Burası Altın Müze Krallığı.",
                "Ve işte buradayız, krallığın kalbinde,",
                "hareketli sokaklarında,",
                "at arabalarının takırtıları,",
                "pazar esnafının bağırışları ve",
                "kraliyet muhafızlarının adımları",
                "yankılanıyor.",
                "Krallığın zengin tarihini ve",
                "kültürel mirasını koruma görevi için buradasınız.",
                "Kral, size krallığın en büyük müzesini",
                "yönetme görevini veriyor. Bu",
                "müze, krallığın geçmişini,",
                "bugününü ve geleceğini barındıracak.",
                "Hoş geldiniz. Krallığın hikayesini",
                "birlikte yazmaya hazır mısınız?"
            }

        };
        SubtitleData ThData = new SubtitleData()
        {
            TargetLanguage = "Thai",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "ยินดีต้อนรับสู่ราชอาณาจักรที่เวลาหยุดนิ่ง",
                "ดินแดนแห่งความงดงามและ",
                "พลังที่สูญหายไปในหน้าประวัติศาสตร์",
                "นี่คือราชอาณาจักรพิพิธภัณฑ์ทองคำ",
                "และที่นี่เราอยู่ในใจกลางของ",
                "ราชอาณาจักร, บนถนนที่คึกคัก",
                "เสียงกระแทกของรถม้า",
                "เสียงตะโกนของพ่อค้าในตลาด และ",
                "เสียงฝีเท้าของทหารรักษาพระองค์",
                "ดังก้อง",
                "คุณมาที่นี่เพื่อทำภารกิจในการ",
                "รักษาประวัติศาสตร์อันรุ่มรวยและ",
                "มรดกทางวัฒนธรรมของราชอาณาจักร",
                "พระราชามอบหมายให้คุณจัดการ",
                "พิพิธภัณฑ์ที่ยิ่งใหญ่ที่สุดของราชอาณาจักร",
                "พิพิธภัณฑ์นี้จะเก็บรักษาอดีต",
                "ปัจจุบันและอนาคตของราชอาณาจักร",
                "ยินดีต้อนรับ. คุณพร้อมที่จะเขียน",
                "เรื่องราวของราชอาณาจักรด้วยกันหรือยัง?"
            }

        };
        SubtitleData SpData = new SubtitleData()
        {
            TargetLanguage = "Spanish",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "Bienvenido a un reino donde el tiempo se detiene,",
                "una tierra de esplendor y",
                "poder perdido en las páginas de la historia.",
                "Este es el Reino del Museo Dorado.",
                "Y aquí estamos en el corazón del",
                "Reino, en las bulliciosas calles,",
                "el ruido de los carruajes tirados por caballos,",
                "los gritos de los vendedores del mercado y",
                "los pasos de los guardias reales",
                "resuenan.",
                "Estás aquí para asumir la tarea de",
                "preservar la rica historia y",
                "herencia cultural del reino.",
                "El Rey te confía la gestión del",
                "mayor museo del Reino. Este",
                "museo albergará el pasado,",
                "presente y futuro del Reino.",
                "Bienvenido. ¿Estás listo para escribir la",
                "historia del Reino juntos?"
            }

        };
        SubtitleData ChWData = new SubtitleData()
        {
            TargetLanguage = "Chinese (Traditional)",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "歡迎來到一個時間停滯的王國，",
                "這是一個在歷史的篇章中",
                "失落的輝煌與力量之地。",
                "這裡是黃金博物館王國。",
                "我們在這裡，王國的心臟，",
                "在繁忙的街道上，",
                "馬車的喧囂，",
                "市場小販的吆喝聲，",
                "以及皇家衛兵的腳步聲",
                "回響。",
                "你在這裡擔任保護",
                "王國豐富的歷史和",
                "文化遺產的任務。",
                "國王委託你管理",
                "王國最偉大的博物館。這個",
                "博物館將會收藏王國的過去、",
                "現在和未來。",
                "歡迎。你準備好一起書寫",
                "王國的故事了嗎？"
            }

        };
        SubtitleData ChSData = new SubtitleData()
        {
            TargetLanguage = "Chinese (Simplified)",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "欢迎来到一个时间静止的王国，",
                "这是一个在历史的篇章中",
                "失落的辉煌与力量之地。",
                "这里是黄金博物馆王国。",
                "我们在这里，王国的心脏，",
                "在繁忙的街道上，",
                "马车的喧嚣，",
                "市场小贩的吆喝声，",
                "以及皇家卫兵的脚步声",
                "回响。",
                "你在这里担任保护",
                "王国丰富的历史和",
                "文化遗产的任务。",
                "国王委托你管理",
                "王国最伟大的博物馆。这个",
                "博物馆将会收藏王国的过去、",
                "现在和未来。",
                "欢迎。你准备好一起书写",
                "王国的故事了吗？"
            }

        };
        SubtitleData KrData = new SubtitleData()
        {
            TargetLanguage = "Kurdish",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "Bî xêr hatî welatekê ku dem rûdine",
                "welatekê xwedan sêva û",
                "hez li rûpelên dîrokê winda bû.",
                "Ev welatê Muzeya Zerîn e.",
                "Û evîdî em li ser navê",
                "welatê, li kolanên bêdeng,",
                "dengê gerîyê tiştên heywanan,",
                "gilîgûlîyên firoşkarên bazarê, û",
                "bingehên derzîxwazên şahanî",
                "girêdayin.",
                "Hûn li vir in da em şîroveya",
                "derbarê dîroka zengîn ên welatê û",
                "mîrasê çandî biparêzin.",
                "Şah, hûn bi vexwendinê ya",
                "welatê mezin derxistin. Ev",
                "muzê, dîroka welatê dîtîya dema xwe,",
                "nuha û paşerojeke bimirîne.",
                "Bî xêr hatî. Hûn amade ne ku",
                "çîrokê ya welatê bi hev re binivîsînin?"
            }

        };
        SubtitleData RsData = new SubtitleData()
        {
            TargetLanguage = "Russian",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "Добро пожаловать в королевство, где время остановилось,",
                "землю великолепия и",
                "силы, утраченные на страницах истории.",
                "Это королевство Золотого Музея.",
                "И вот мы в сердце",
                "королевства, на оживленных улицах,",
                "грохот конных повозок,",
                "крики рыночных торговцев и",
                "шаги королевских стражей",
                "эхом раздаются.",
                "Вы здесь, чтобы взять на себя задачу",
                "сохранения богатой истории и",
                "культурного наследия королевства.",
                "Король поручает вам управление",
                "великим музеем королевства. Этот",
                "музей будет хранить прошлое,",
                "настоящее и будущее королевства.",
                "Добро пожаловать. Готовы ли вы вместе",
                "писать историю королевства?"
            }

        };
        SubtitleData DuData = new SubtitleData()
        {
            TargetLanguage = "German",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "Willkommen in einem Königreich, in dem die Zeit stillsteht,",
                "ein Land des Glanzes und",
                "der Macht, verloren in den Seiten der Geschichte.",
                "Dies ist das Königreich des Goldenen Museums.",
                "Und hier sind wir im Herzen des",
                "Königreichs, in den belebten Straßen,",
                "das Klappern der Pferdekutschen,",
                "die Rufe der Marktverkäufer und",
                "die Schritte der königlichen Wachen",
                "hallen wider.",
                "Du bist hier, um die Aufgabe zu übernehmen,",
                "die reiche Geschichte und",
                "das kulturelle Erbe des Königreichs zu bewahren.",
                "Der König vertraut dir die Verwaltung des",
                "größten Museums des Königreichs an. Dieses",
                "Museum wird die Vergangenheit,",
                "Gegenwart und Zukunft des Königreichs beherbergen.",
                "Willkommen. Bist du bereit, gemeinsam die",
                "Geschichte des Königreichs zu schreiben?"
            }

        };
        SubtitleData FrData = new SubtitleData()
        {
            TargetLanguage = "French",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "Bienvenue dans un royaume où le temps s'arrête,",
                "une terre de splendeur et",
                "de pouvoir perdu dans les pages de l'histoire.",
                "Ceci est le Royaume du Musée d'Or.",
                "Et ici, nous sommes au cœur du",
                "Royaume, dans les rues animées,",
                "le cliquetis des carrosses,",
                "les cris des vendeurs du marché et",
                "les pas des gardes royaux",
                "résonnent.",
                "Vous êtes ici pour prendre en charge la tâche de",
                "préserver l'histoire riche et",
                "le patrimoine culturel du royaume.",
                "Le Roi vous confie la gestion du",
                "plus grand musée du Royaume. Ce",
                "musée abritera le passé,",
                "le présent et l'avenir du Royaume.",
                "Bienvenue. Êtes-vous prêt à écrire ensemble",
                "l'histoire du Royaume ?"
            }

        };
        SubtitleData JpData = new SubtitleData()
        {
            TargetLanguage = "Japanese",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "時が止まる王国へようこそ",
                "壮麗と",
                "歴史のページに失われた力の地。",
                "ここはゴールデンミュージアムの王国です。",
                "そしてここに私たちはいます、",
                "王国の中心で、",
                "賑やかな通りに、",
                "馬車のガタガタ音、",
                "市場の売り手の叫び声、そして",
                "王室の警備兵の足音が",
                "響き渡ります。",
                "あなたはここで王国の豊かな歴史と",
                "文化遺産を守る任務を担っています。",
                "王はあなたに",
                "王国の最大の博物館を管理する任務を託します。この",
                "博物館は王国の過去、",
                "現在、そして未来を収蔵するでしょう。",
                "ようこそ。共に王国の物語を",
                "書く準備はできていますか？"
            }

        };
        SubtitleData KoData = new SubtitleData()
        {
            TargetLanguage = "Korean",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "시간이 멈춘 왕국에 오신 것을 환영합니다,",
                "찬란함과",
                "역사의 페이지에 잃어버린 힘의 땅.",
                "여기는 골든 뮤지엄의 왕국입니다.",
                "그리고 여기 우리는 왕국의",
                "심장부에 있습니다,",
                "번화한 거리에서,",
                "말이 끄는 마차의 덜컹거림,",
                "시장 상인의 외침, 그리고",
                "왕실 경비원의 발걸음이",
                "울려 퍼집니다.",
                "당신은 여기서 왕국의 풍부한 역사와",
                "문화 유산을 보존하는 임무를 맡았습니다.",
                "왕은 당신에게",
                "왕국의 가장 큰 박물관을 관리하는 임무를 맡깁니다. 이",
                "박물관은 왕국의 과거,",
                "현재 및 미래를 담을 것입니다.",
                "환영합니다. 함께 왕국의 이야기를",
                "써 나갈 준비가 되었나요?"
            }

        };
        SubtitleData ArData = new SubtitleData()
        {
            TargetLanguage = "Arabic",
            BeginEndSeconds = new List<Vector2>
            {
                new Vector2(0.080f, 2.720f),
                new Vector2(2.720f, 5.320f),
                new Vector2(5.320f, 8.080f),
                new Vector2(8.720f, 11.52f),
                new Vector2(13.04f, 15.52f),
                new Vector2(15.52f, 18.16f),
                new Vector2(18.48f, 20.1f),
                new Vector2(20.1f , 23.6f),
                new Vector2(23.6f, 26.08f),
                new Vector2(26.08f, 26.64f),
                new Vector2(26.99f, 28.75f),
                new Vector2(28.75f, 31.35f),
                new Vector2(31.35f, 32.67f),
                new Vector2(32.912f, 35.152f),
                new Vector2(35.152f,37.792f),
                new Vector2(37.792f,40.352f),
                new Vector2(40.832f,43.632f),
                new Vector2(45.072f,47.832f),
                new Vector2(47.832f,49.632f)
            },
            Text = new List<string>
            {
                "مرحبًا بكم في مملكة حيث يتوقف الزمن",
                "أرض الروعة و",
                "القوة الضائعة في صفحات التاريخ.",
                "هذه هي مملكة المتحف الذهبي.",
                "وها نحن هنا في قلب",
                "المملكة، في الشوارع الصاخبة،",
                "صوت عربة الخيول،",
                "صياح بائعي السوق، و",
                "خطوات الحراس الملكيين",
                "تتردد.",
                "أنت هنا لتولي مهمة",
                "حفظ التاريخ الغني للمملكة و",
                "تراثها الثقافي.",
                "الملك يثق بك لإدارة",
                "أعظم متحف في المملكة. هذا",
                "المتحف سيحتوي على الماضي،",
                "الحاضر والمستقبل للمملكة.",
                "مرحبًا بك. هل أنت مستعد لكتابة",
                "قصة المملكة معًا؟"
            }
        };
        subtitleDatas.Add(EnData);
        subtitleDatas.Add(TrData);
        subtitleDatas.Add(ThData);
        subtitleDatas.Add(SpData);
        subtitleDatas.Add(ChWData);
        subtitleDatas.Add(ChSData);
        subtitleDatas.Add(KrData);
        subtitleDatas.Add(RsData);
        subtitleDatas.Add(RsData);
        subtitleDatas.Add(DuData);
        subtitleDatas.Add(FrData);
        subtitleDatas.Add(JpData);
        subtitleDatas.Add(KoData);
        subtitleDatas.Add(ArData);
    }
    public SubtitleData GetCurrentLanguageSubtitleData()
    {
        Debug.Log("Current Language => " + LocalizationManager.CurrentLanguage);
        string currentLanguage = LocalizationManager.CurrentLanguage;
        SubtitleData sd = subtitleDatas.Where(x => x.TargetLanguage == currentLanguage).SingleOrDefault();
        return sd;
    }
    private void OnVideoEnd(VideoPlayer source)
    {
        foreach (var item in CloseEndVideo)
            item.gameObject.SetActive(false);
        TutorialLevelManager.instance.OnEndFlyCutscene();
    }

    void OnPlayableDirectorStopped(PlayableDirector director)
    {
        Debug.Log("Timeline has ended.");
        OnEndEnterCutscene();
    }

    public void OnEndEnterCutscene()
    {
        //currentActiveCutscene.stopped -= OnPlayableDirectorStopped;
        //TutorialLevelManager.instance.OnEndFlyCutscene();
    }

    bool endVideo = false;
    private void OnNewFrame()
    {
        if (endVideo)
            return;

        double currentTime = videoPlayer.time;
        bool timeBool = false;
#if UNITY_EDITOR
        timeBool = currentTime > 3; //default 3 or 5
#else
        timeBool = currentTime > 49.7f;
#endif
        if (!endVideo && timeBool) // Yayinlanmadan once kod defulta cekilmeli => DefaultValue: 49.7f
        {
            endVideo = true;
            OnVideoEnd(null);
            return;
        }

        // Access the current time in seconds
        bool active = false;
        foreach (var item in subtitleData.BeginEndSeconds)
        { 
            if (currentTime > item.x && currentTime < item.y)
            {
                active = true;
                SubtitleText.text = subtitleData.Text[subtitleData.BeginEndSeconds.IndexOf(item)];
                if (!SubtitleCanvas.gameObject.activeSelf)
                {
                    SubtitleCanvas.gameObject.SetActive(true);
                    StopAllCoroutines();
                    StartCoroutine(FadeIn(SubtitleCanvas));
                }
                else
                {
                    if (!fadein)
                    {
                        StopAllCoroutines();
                        StartCoroutine(FadeIn(SubtitleCanvas));
                    }
                }
            }
        }
        if (!active)
        {
            if (SubtitleCanvas.gameObject.activeSelf)
            {
                if (!fadeout)
                {
                    StopAllCoroutines();
                    StartCoroutine(FadeOut(SubtitleCanvas));
                }
            }
            else
            {
                SubtitleText.text = "";
            }
        }
    }

    void Update()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            OnNewFrame();
        }
    }

    bool fadein;
    bool fadeout;
    IEnumerator FadeOut(CanvasGroup c)
    {
        fadeout = true;
        while (c.alpha > 0)
        {
            c.alpha -= Time.deltaTime * 3f;
            if (c.alpha < 0)
                c.alpha = 0;

            yield return new WaitForEndOfFrame();
        }

        c.alpha = 0;
        c.gameObject.SetActive(false);
        fadeout = false;
    }

    IEnumerator FadeIn(CanvasGroup c)
    {
        fadein = true;
        while (c.alpha < 1)
        {
            c.alpha += Time.deltaTime * 3f;
            if (c.alpha > 1)
                c.alpha = 1;

            yield return new WaitForEndOfFrame();
        }

        c.alpha = 1;
        fadein = false;
    }

    public struct SubtitleData
    {
        public string TargetLanguage;
        public List<Vector2> BeginEndSeconds;
        public List<string> Text;
    }
}
