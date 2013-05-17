using GalaSoft.MvvmLight;
using M0rg0tRss.Data;
using M0rg0tRss.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Web.Syndication;

namespace M0rg0tRss.ViewModel
{
    public class RssViewModel : ViewModelBase
    {
        private bool _loading = false;
        public bool Loading
        {
            get
            {
                return _loading;
            }
            set
            {
                if (_loading != value)
                {
                    _loading = value;
                    RaisePropertyChanged("Loading");
                };
            }
        }

        public async Task<bool> LoadCacheRss(StorageFile[] feedsEntries)
        {
            Loading = true;
            AddTourist();

            foreach (var feed in feedsEntries)
            {
                //await ViewModelLocator.MainStatic.AddGroupForFeedAsync(feed.url, feed.id);
                await ViewModelLocator.MainStatic.AddGroupForFeedAsync(feed);
            }
            RaisePropertyChanged("AllGroups");
            Loading = false;
            return true;
        }

        public async void LoadRss()
        {
            Loading = true;
            //AddTourist();

            var feeds = await App.ReadSettings();

            foreach (var feed in feeds)
            {
                await ViewModelLocator.MainStatic.AddGroupForFeedAsync(feed.url, feed.id);
            }

            /*await ViewModelLocator.MainStatic.AddGroupForFeedAsync("http://rybinsk.ru/news-2013?format=feed");
            await ViewModelLocator.MainStatic.AddGroupForFeedAsync("http://rybinsk.ru/afisha?format=feed");
            await ViewModelLocator.MainStatic.AddGroupForFeedAsync("http://rybinsk.ru/sport-rybinsk?format=feed");
            await ViewModelLocator.MainStatic.AddGroupForFeedAsync("http://rybinsk.ru/economy/market?format=feed");
            await ViewModelLocator.MainStatic.AddGroupForFeedAsync("http://rybinsk.ru/admin/division/security-nature/jekologija?format=feed");*/
            RaisePropertyChanged("AllGroups");
            Loading = false;
        }

        private ObservableCollection<RssDataGroup> _allGroups = new ObservableCollection<RssDataGroup>();
        public ObservableCollection<RssDataGroup> AllGroups
        {
            get
            {
                ObservableCollection<RssDataGroup> tempGroups = new ObservableCollection<RssDataGroup>();
                var sorted = (from groupitem in _allGroups
                              orderby groupitem.Order descending
                              select groupitem).ToList();
                foreach(var item in sorted) {
                    tempGroups.Add(item);
                };
                return tempGroups;
                //return _allGroups;
            }
            set
            {
                if (_allGroups!=value)
                {
                    _allGroups = value;
                    RaisePropertyChanged("AllGroups");
                }
            }
        }

        public async Task<bool> AddGroupForFeedAsync(StorageFile sf)
        {
            string clearedContent = String.Empty;

            if (GetGroup(sf.DisplayName) != null) return false;

            var feed = new SyndicationFeed();
            feed.LoadFromXml(await XmlDocument.LoadFromFileAsync(sf));

            var feedGroup = new RssDataGroup(
                uniqueId: sf.DisplayName.ToString().Replace(".rss", ""),
                title: feed.Title != null ? feed.Title.Text : null,
                subtitle: feed.Subtitle != null ? feed.Subtitle.Text : null,
                imagePath: feed.ImageUri != null ? feed.ImageUri.ToString() : null,
                description: null);

            foreach (var i in feed.Items)
            {
                string imagePath = GetImageFromPostContents(i);

                if (i.Summary != null)
                    clearedContent = i.Summary.Text;
                else
                    if (i.Content != null)
                        clearedContent = i.Content.Text;

                if (imagePath != null && feedGroup.Image == null)
                    feedGroup.SetImage(imagePath);

                if (imagePath == null) imagePath = "ms-appx:///Assets/DarkGray.png";

                feedGroup.Items.Add(new RssDataItem(
                    uniqueId: i.Id, title: i.Title.Text, subtitle: null, imagePath: imagePath,
                    description: null, content: clearedContent, @group: feedGroup));
            }

            _allGroups.Add(feedGroup);
            return true;
        }

        public async Task<bool> AddGroupForFeedAsync(string feedUrl, string ID="1")
        {
            string clearedContent = String.Empty;

            if (GetGroup(feedUrl) != null) return false;

            var feed = await new SyndicationClient().RetrieveFeedAsync(new Uri(feedUrl));

            var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync
                ("Data", CreationCollisionOption.OpenIfExists);
            //получаем/перезаписываем файл с именем "ID".rss
            var fileToSave = await localFolder.CreateFileAsync(ID + ".rss", CreationCollisionOption.ReplaceExisting);

            //сохраняем фид в этот файл
            await feed.GetXmlDocument(SyndicationFormat.Rss20).SaveToFileAsync(fileToSave);

            var feedGroup = new RssDataGroup(
                uniqueId: ID,
                title: feed.Title != null ? feed.Title.Text : null,
                subtitle: feed.Subtitle != null ? feed.Subtitle.Text : null,
                imagePath: feed.ImageUri != null ? feed.ImageUri.ToString() : null,
                description: null);

            foreach (var i in feed.Items)
            {
                string imagePath = null;
                try
                {
                    imagePath = GetImageFromPostContents(i); ;
                }
                catch { };

                if (i.Summary != null)
                    clearedContent = Windows.Data.Html.HtmlUtilities.ConvertToText(i.Summary.Text);
                else
                    if (i.Content != null)
                        clearedContent = Windows.Data.Html.HtmlUtilities.ConvertToText(i.Content.Text);

                if (imagePath != null && feedGroup.Image == null)
                    feedGroup.SetImage(imagePath);

                if (imagePath == null) imagePath = "ms-appx:///Assets/DarkGray.png";

                try
                {
                    feedGroup.Items.Add(new RssDataItem(
                        uniqueId: i.Id, title: i.Title.Text, subtitle: null, imagePath: imagePath,
                        description: null, content: clearedContent, @group: feedGroup));
                }
                catch { };
            }

            switch (feedGroup.UniqueId)
            {
                case "1":
                    feedGroup.Order = 20;

                    try
                    {
                        var group1 = new RssDataGroup("MainNews", "Главная новость", "", "", "");
                        group1.Order = 30;
                        var tempitem = new RssDataItem(feedGroup.Items.First().UniqueId + "main",
                        feedGroup.Items.First().Title, null,
                        feedGroup.Items.First()._imagePath,
                        "",
                        feedGroup.Items.First().Content,
                        group1);

                        group1.Items.Add(tempitem);
                        group1.Items.Add(tempitem);
                        group1.Items.Add(tempitem);
                        group1.Items.Add(tempitem);
                        group1.Items.Add(tempitem);
                        group1.Items.Add(tempitem);

                        _allGroups.Remove(_allGroups.FirstOrDefault(c => c.UniqueId == feedGroup.UniqueId));
                        _allGroups.Add(group1);
                        
                        //AllGroups = SortItems();
                    }
                    catch { };
                    break;
            };

            _allGroups.Remove(_allGroups.FirstOrDefault(c=>c.UniqueId == feedGroup.UniqueId));
            _allGroups.Add(feedGroup);
            //AllGroups = SortItems();
            return true;
        }

        private ObservableCollection<RssDataGroup> SortItems()
        {
            ObservableCollection<RssDataGroup> tempGroups = new ObservableCollection<RssDataGroup>();
            var sorted = (from groupitem in _allGroups
                          orderby groupitem.Order descending
                          select groupitem).ToList();
            foreach (var item in sorted)
            {
                tempGroups.Add(item);
            };
            return tempGroups;
        }

        private static string GetImageFromPostContents(SyndicationItem item)
        {
            string text2search = "";

            if (item.Content != null) text2search += item.Content.Text;
            if (item.Summary != null) text2search += item.Summary.Text;

            return Regex.Matches(text2search,
                    @"(?<=<img\s+[^>]*?src=(?<q>['""]))(?<url>.+?)(?=\k<q>)",
                    RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Where(m =>
                {
                    Uri url;
                    if (Uri.TryCreate(m.Groups[0].Value, UriKind.Absolute, out url))
                    {
                        string ext = Path.GetExtension(url.AbsolutePath).ToLower();
                        if (ext == ".png" || ext == ".jpg" || ext == ".bmp") return true;
                    }
                    return false;
                })
                .Select(m => m.Groups[0].Value)
                .FirstOrDefault();
        }

        public IEnumerable<RssDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            return AllGroups;
        }

        public RssDataGroup GetGroup(string uniqueId)
        {
            // Для небольших наборов данных можно использовать простой линейный поиск
            var matches = AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public RssDataItem GetItem(string uniqueId)
        {
            // Для небольших наборов данных можно использовать простой линейный поиск
            var matches = AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() > 0) return matches.First();
            return null;
        }

        public RssViewModel()
        {            
        }

        private MapItem _currentTouristItem = null;
        public MapItem CurrentTouristItem
        {
            get
            {
                return _currentTouristItem;
            }
            set
            {
                if (_currentTouristItem!=value) {
                    _currentTouristItem = value;
                    RaisePropertyChanged("CurrentTouristItem");
                };
            }
        }

        public void AddTourist()
        {
            var tourist = new RssDataGroup("Tourist",
                    "Достопримечательности Рыбинска",
                    "",
                    "Assets/sobor-volga.jpg",
                    "");
            tourist.Order = 5;

            tourist.Items.Add(new MapItem("Group-1-Item-1",
                    "Спасо-Преображенский собор - описание",
                    "",
                    "Assets/sobor-volga.jpg",
                    "Спасо-Преображенский собор – жемчужина исторического центра города Рыбинска.",
                    "Спасо-Преображенский собор – жемчужина исторического центра города Рыбинска.\nПрородителем Спасо-Преображенского собора была деревянная церковь в честь апостола Петра, покровителя рыбаков. В XVII веке на месте обветшалой деревянной построили каменную, во  имя Преображения Господня, которая стала соборной в 1778 году, затем ее перестроили и в 1804 году рядом с ней возвели новую, каменную колокольню. Взору горожан предстала устремленная в небо почти стометровая свеча, украшенная колоннами.\nГород рос, росло и количество православных жителей. Вопрос о строительстве в Рыбинске более вместительного собора возник еще в самом начале XIX века, но по разным причинам откладывался, и решение затянулось на двадцать пять лет. В 1838 году старый собор все-таки разбирают и начинают возводить новый, который потом назовут «красой Поволжья».\nОказывается, что проект собора, прежде чем оказался в Рыбинске, участвовал в конкурсе на строительство Исаакиевского собора в Петербурге и занял там третье место. В 18831-1851 гг. по проекту группы петербургских архитекторов во главе с А. И. Мельниковым на месте одноимённой церкви XVII века Петра и Павла был воздвигнут Спасо-Преображенский собор. Работы по строительству шли тринадцать лет, но результат превзошел все ожидания. Огромное здание в форме куба венчали мощные купола, опирающиеся на массивные барабаны, северный и южный фасады украшали высокие колонны.\nВнутреннее убранство собора также ошеломляло своей идее. Пол выложен гранитными плитами, стены отделаны под белый мрамор. Для оформления интерьера израсходовали более пятисот килограммов позолоченного серебра. Мастер из Ростова Великого В.М. Бычков сделал для собора огромный четырехъярусный иконостас, в котором были древние иконы XV века. В центре собора под бархатным балдахином поставили главную реликвию города — нарядное кресло, сделанное специально к приезду Екатерины II.\nСегодня Спасо-Преображенский собор воспринимается как единый архитектурный ансамбль, созданный по единому проекту. Однако пятиярусная колокольня построена на 50 лет раньше самого храма, предположительно по проекту костромского зодчего-самоучки Степана Воротилова. Её архитектуре присуща и мягкая живописность барокко, и изящная сдержанность классицизма, в отличие от величавой архитектуры Собора.",
                    tourist, 58.048353, 38.858850));

            tourist.Items.Add(new MapItem("Group-2-Item-1",
                    "Памятник Бурлаку",
                    "",
                    "Assets/byrlakryb_thumb.jpg",
                    "Памятник Бурлаку",
                    "Рыбинск когда-то именовался столицей бурлаков. Поэтому не удивительно, что на Волжской набережной в Рыбинске на самом видном месте, недалеко от Рыбинского историко-архитектурного и художественного музея-заповедника, находится памятник Бурлаку. Скульптура установлена на Стоялой улице в 1977 году к 200-летию города.\nПисаревский (автор памятника) мечтал создать для Рыбинска скульптуру могучего труженика. Но не успел осуществить свой замысел.\nПродолжил задуманное друг скульптора М.Е. Удалеев. Он сумел организовать установку модели на Стоялой улице, где некогда толпились тысячи бурлаков, крючников.\nЗдесь скульптура простояла немного лет и ему пришлось уступить место адмиралу Ф.Ф. Ушакову. Бурлака переместили к старой бирже. Интересно узнать, что эта скульптура лишь этюд, который предназначался для музейного зала.\nСкульптура очень сильная, изображает мужественного героя этой тяжелейшей профессии. Сидит усталый бурлак, по-волжски «зимогор», на большом камне и смотрит задумчиво на волжские просторы. Для него река – и кормилица, и нескончаемый труд.\nТрудно теперь представить, что когда-то эти обычные мужики, в большинстве своем крестьяне или бродяги-босяки, буквально на своих плечах тянули груженые баржи! А теперь у скульптуры Бурлака обнимаются влюбленные парочки, играют дети, назначают приятные встречи. Впрочем, так было и в давние времена. Чтобы кому-то веселиться и развлекаться, бурлаки весь сезон тянули свою нелегкую ношу.\nАдрес: Скульптура находится в парковой зоне на Волжской набережной, у старой «хлебной» (лоцманской) биржи  (Волжская наб., д. 4)",
                    tourist, 58.04983, 38.85418));

            tourist.Items.Add(new MapItem("Group-3-Item-1",
                    "Старая хлебная биржа",
                    "",
                    "Assets/birzhastar.jpg",
                    "Старая хлебная биржа",
                    "Одним из зданий, которое встречает туристов на берегу Волги в Рыбинске, является старая «лоцманская» биржа. Строгие пропорции делают это здание одним из лучших памятников провинциального классицизма. Раньше купцы всего Рыбинска торговали здесь хлебом, заключали выгодные сделки. Но так было не всегда.\nСтарая хлебная биржа Рыбинска хранит интересную историю, которая заслуживает освещения на страницах путеводителя.\nАвтором хлебной биржи является архитектор Герасим Варфоломеевич Петров.\nОткрытие состоялось в 1811 году – 18 июля. Кстати средства нашлись за счет благотворительных взносов иногородних купцов, торгующих в Рыбинске (стоимость здания 21437 руб. 58 коп.) На открытии присутствовал сам губернатор Голицин Михаил Николаевич.\nПосле открытия оказалось, что новое здание биржи пустовало, из людей сюда приходили только любопытствующие. И Рыбинская городская дума решила использовать здание под свои нужды. Не занимая большого биржевого зала, она разместила свои городские учреждения там. В 1830-1831 годах после ремонта помещений в биржу заехали уездный и земские суды.\nВ 1841 году проезжая через Рыбинск Николай I дал распоряжение вновь открыть биржу и дать ей надлежащее устройство. Однако и это не убедило рыбинских купцов и все сделки заключались на улицах, в торговых лавках и за чаем.\nВ 1860 году председателем биржи стал М. Н. Журавлев, после чего биржа стала использовать для торговли хлебом и в последующие годы стала крупнейшей в России.\nКак биржа здание существовало до 2 октября 1912 года.  В 1912 стал решать вопрос о строительстве новой биржи, а что же делать со старой? Здание требовало ремонта и шли споры о его дальнейшем назначении. Планировалось даже сдавать его в аренду для торговли. Срочное размещение двух рот Гроховского полка, расквартированного в городе, сняли все вопросы и биржа была занята военными. В советское время здание занимали различные учебные заведения. В послевоенные годы размещались речной вокзал и водная милиция.\nВ последнее время здание старой биржи находится на балансе Рыбинского государственного историко-архитектурного и художественного музея-заповедника. Вскоре здесь планируется открыть экспозицию по истории волжского судоходства и Рыбинской пристани.\n",
                    tourist, 58.04966, 38.85487));

            tourist.Items.Add(new MapItem("Group-4-Item-1",
                    "Рыбинское водохранилище",
                    "",
                    "Assets/rybinskoe-reservior.jpg",
                    "Рыбинское водохранилище",
                    "Рыбинское водохранилище – самое большое рукотворное море в Европе.\nПосле его создания Рыбинск получил выход в пять морей. Рыбинское водохранилище привлекает рыбаков и любителей отдыха на воде, позволяет развиваться промышленному рыболовству.  Интересна и ловля рыбы в реке у мест впадения рек в водохранилище.\nМоре в пятнадцать раз больше Московского и превосходит по площади многие водоемы нашей страны.\nНа северо-западном побережье Рыбинского водохранилища расположен Дарвинский заповедник, где водятся медведи, куницы, горностаи, гнездится 230 видов птиц, расположен прекрасный музей природы.\n",
                    tourist, 58.30227, 38.51807));

            tourist.Items.Add(new MapItem("Group-5-Item-1",
                    "Рыбинский мост",
                    "",
                    "Assets/IMG_0257.jpg",
                    "Рыбинский мост",
                    "Рыбинский мост — уникальная архитектурная постройка и он, на сегодняшний день, является одним  из красивейших мостов на Волге. Мост в в сочетании со Спасо-Преображенским собором без сомнения является самой узнаваемой достопримечательностью Рыбинска.\n\nИстория моста в Рыбинске\n\nЕще в далеком 1938 году сметой Волгостоя предусматривалось строительство Рыбинского моста. Его архитектором является Уланов (родной брат всемирно известной балерины). В 1939 году была установлена часть бетонных опор моста. А в 1941 строительство было остановлено из-за начала Великой Отечественной войны.\nДалее в 1955 году, как сообщает Рыбинский календарь, строительство было возобновлено. К месту строительства были подведены железнодорожный мост, а на берегу Волги построен военный завод. Поставка всех материалов осуществлялась через железнодорожное сообщение Ярославль – Рыбинск, с устройством временного деревянного моста чрез реку Черемуху.\nСтроительство шло успешно, и в 1959 году было закончено строительство 4 опор. Установлено одно пролетное строение, велись работы на подходах к мосту. Представьте, за 3 года был потрачен 21 миллион рублей на строительство. На эти же средства был возведен портальный кран грузоподъемностью в 60 тонн.\nВ августе 1963 года строительство было закончено и 27 августа мост был принят с рядом недоделок, которые устранялись еще 2 года.",
                    tourist, 58.04972, 38.86102));

            tourist.Items.Add(new MapItem("Group-6-Item-1",
                    "Рыбинский музей-заповедник (новая хлебная биржа)",
                    "",
                    "Assets/IMG_01361.jpg",
                    "Рыбинский музей-заповедник (новая хлебная биржа)",
                    "Рыбинский музей-заповедник (подробнее о музее) — один из лучших на Волге музеев, распологающий свыше 100 тысяч единиц экспонатов.\n\nЗдание новой биржи построено в 1912 году в «неорусском» стиле, использовавшем стилизованные черты древнерусской архитектуры, по проекту архитектора Александра Васильевича Иванова.\nНа момент проектирования биржи Александр Иванов занимал должность архитектора московского Кремля. Строительство новой биржи было продиктовано возросшим авторитетом Рыбинской биржи в торговых кругах России.\nНеоднократно высшие правительственные учреждения запрашивали мнение биржевого комитета по различным экономическим вопросам. Постепенно прежнее здание биржи становится тесным и на берегу Волги появляется величественное здание новой «хлебной» биржи.\nСегодня в здании располагается Рыбинский государственный историко-архитектурный и художественный музей-заповедник.\n",
                    tourist, 58.04948, 38.85629));

            tourist.Items.Add(new MapItem("Group-7-Item-1",
                    "Дом художников",
                    "",
                    "Assets/dom-hydoznikov.jpg",
                    "Дом художников",
                    "Прекрасным образцом деревянной архитектуры начала ХХ века является двухэтажное здание, расположенное на углу улиц Пушкина и Плеханова (ул. Пушкина, №52), фотографии которого украшают современные буклеты и проспекты, рассказывающие об историческом Рыбинске.\nВ 1900 году С.Г. Гордеев выстроил этот наугольный дом, который двумя фасадами выходил на обе улицы. Находившийся поблизости железнодорожный вокзал придавал особую значимость этому бойкому перекрестку, и домовладелец, конечно, постарался выстроить здесь такую «хоромину», которую не стыдно было выставить напоказ на людном месте.\nНад угловой частью обшитого тесом особняка возвышались два высоких шатра, каждый из которых был ориентирован на свою улицу и имел очень сложное завершение в виде крещатой бочки  с коваными навершиями. Особый колорит перекрестку улиц придавал внушительный балкон второго этажа с кованой ажурной решеткой, полукругом объединявший фасады дома. Украшением дома были и наличники окон, условно повторяющих формы нарышкинского барокко.\nНедавно этот уникальный дом был реконструирован, с сохранением деревянной обшивки и декорации.\n",
                    tourist, 58.04363, 38.83647));

            tourist.Items.Add(new MapItem("Group-8-Item-1",
                    "Никольская часовня",
                    "",
                    "Assets/nicolsk.jpg",
                    "Никольская часовня",
                    "Раньше, спускаясь по Стоялой улице в Рыбинске, случайных прохожий вряд ли обратил бы внимание на Никольскую часовню. Сегодня это двухэтажное незаметное сооружение с неказистым видом сегодня приняло новый облик. На фотографии выше вы можете видеть здание еще в процессе реставрации.\nУже сейчас на главе часовни установлен крест. Кроме главного креста 10 сентября установлены шесть небольших крестов на главках, украшающих купол часовни. Позднее появятся еще шесть. Все кресты отлиты фирмой «Ярославский реставратор» по правилам, действующим в 19 веке, и покрыты сусальным золотом.\nКак сообщает официальный сайт города, полная реставрация закончатся к маю 2011 года.\n",
                    tourist, 58.05019, 38.85255));

            this._allGroups.Add(tourist);
        }

    }
}
