using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;


namespace RPA
{
    class Program
    {
        static IWebDriver MyMozila;
        static String[] UrlSite = new String[] { "https://msk.tele2.ru/", "https://rostov.tele2.ru/", "https://more.tele2.ru/" };
        static String Login,
                      Password;
        static public void Zadanie1()
        {
            int TarifNow,
                NumIsStrong = 0,
                NumIsNotStrong = 0;
            MyMozila.Navigate().GoToUrl(UrlSite[0]);
            var wait = new WebDriverWait(MyMozila, TimeSpan.FromSeconds(15));
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.ClassName("hit-image")));
            //MyMozila.get(UrlSite[0]);
            Console.WriteLine("Выполняю - Проверить «жирный» текст в любом тарифе или обычный");
            //поиск тарифов
            var TarifArray = MyMozila.FindElements(By.CssSelector(".ssc-tariffs-wrap .ssc-tariff-box"));
            Console.WriteLine("На странице найдено " + TarifArray.Count + " тарифов. Какой выбрать? ");
            TarifNow = Convert.ToInt32(Console.ReadLine());
            //узнаем есть ли в блоке тарифа тег strong
            Console.WriteLine("На странице найдено 'жирного'(strong) текста " +
                TarifArray[TarifNow - 1].FindElements(By.TagName("strong")).Count);
            //узнаем есть ли в блоке тарифа текстовые блоки со стилем жирный или без

            var TextElementArray = TarifArray[TarifNow - 1].FindElements(By.TagName("span"));

            foreach (var NowElement in TextElementArray)
            {
                if (NowElement.FindElements(By.TagName("span")).Count > 0) continue;
                Int32 NowWeight = 0;
                try
                {
                    NowWeight = Convert.ToInt32(NowElement.GetCssValue("font-weight"));

                }
                catch (FormatException)
                {
                    NowWeight = 400;
                }

                if (NowWeight > 400 || NowElement.GetCssValue("font-weight") == "bold")
                {
                    NumIsStrong++;
                    //Console.WriteLine("Жир - " + NowElement.Text);
                }
                else
                {
                    NumIsNotStrong++;
                    //Console.WriteLine("Норм - " + NowElement.Text);
                }
            }
            Console.WriteLine("Жирных блоков текста (css свойство) - " + NumIsStrong + " обычных блоков текста - " + NumIsNotStrong);

            Console.WriteLine("Выполняю - 2.Проверить тарифы на наличие стикера «ХИТ продаж»");
            foreach (var item in TarifArray)
            {
                try
                {
                    if (item.FindElement(By.ClassName("hit-image")).GetAttribute("src").Length > 0)
                    {
                        Console.WriteLine("тариф хит - " + item.FindElement(By.ClassName("tariff-title")).Text);
                        break;
                    }
                }
                catch (NoSuchElementException)
                {
                    //Console.WriteLine("тариф не наш - " + item.FindElement(By.ClassName("tariff-title")).Text);
                }
            }

            Console.WriteLine("Выполняю - 3.	Проверить стоимости тарифов на главной странице и на странице тарифа");
            String BuferString = "";
            for (int i = 1; i <= TarifArray.Count; i++)
            {
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".ssc-tariffs-wrap .ssc-tariff-box")));
                var TarifNowElement = MyMozila.FindElement(By.CssSelector(".ssc-tariffs-wrap .ssc-tariff-box:nth-child(" + i.ToString() + ")"));
                BuferString = TarifNowElement.FindElement(By.ClassName("tariff-title")).Text +
                    " на главной " + TarifNowElement.FindElement(By.ClassName("price")).Text + ". На странице тарифа ";
                TarifNowElement.Click();// переход на страницу тарифа. 
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".btn-col .price")));
                //Thread.Sleep(2000); //костыль. если опять будет при первом запуске гдето показывать не правильную стоимость на странице тарифа
                Console.WriteLine(BuferString + MyMozila.FindElement(By.CssSelector(".btn-col .price")).Text);
                MyMozila.Navigate().Back();
            }

            Console.WriteLine("Выполняю - 4.	Проверить стоимости тарифов на 2х любых под сайтах ТЕЛЕ2, например в Москве (https://msk.tele2.ru/) и Ростове-на-Дону (https://rostov.tele2.ru/)");
            // тут MyMozila созданим новую вкладку (окно)
            IWebDriver MyMozila2 = new FirefoxDriver();
            MyMozila2.Navigate().GoToUrl(UrlSite[1]);
            TarifArray = MyMozila.FindElements(By.CssSelector(".ssc-tariffs-wrap .ssc-tariff-box"));
            var TarifArray2 = MyMozila2.FindElements(By.CssSelector(".ssc-tariffs-wrap .ssc-tariff-box"));
            String TarifNowName,
                   TarifNowPrice;
            //сравним тарифы на подсайтах
            foreach (var item1 in TarifArray)
            {
                TarifNowName = item1.FindElement(By.ClassName("tariff-title")).Text;
                TarifNowPrice = item1.FindElement(By.ClassName("price")).Text;
                Console.WriteLine("тариф для сравнения (msk) - " + TarifNowName);
                foreach (var item2 in TarifArray2)
                {
                    if (item2.FindElement(By.ClassName("tariff-title")).Text == TarifNowName)
                    {
                        Console.WriteLine("Стоимость в (msk) - " + TarifNowPrice + " в (rostov) " + item2.FindElement(By.ClassName("price")).Text);
                        break;
                    }
                }
            }
            MyMozila2.Close();

            Console.WriteLine("5.	Проверить – можно ли настраивать каждый из тарифов или нет");
            TarifArray = MyMozila.FindElements(By.CssSelector(".ssc-tariffs-wrap .ssc-tariff-box"));
            foreach (var ItemTarif in TarifArray)
            {
                TarifNowName = ItemTarif.FindElement(By.ClassName("tariff-title")).Text;
                try
                {
                    if (ItemTarif.FindElement(By.CssSelector("a[href^='/nastroy']")).Text.Length > 0)
                     Console.WriteLine(TarifNowName + " можно настроить"); 
                    else
                     Console.WriteLine(TarifNowName + " нельзя настроить"); 
                }
                catch (NoSuchElementException)
                {//FormatException
                    Console.WriteLine(TarifNowName + " нельзя настроить");
                }
            }

            Console.WriteLine("6.	Вывести в консоль все ссылки на картинки с страницы https://more.tele2.ru/ (огнетушитель, самолет и т.д.)");
            MyMozila.Navigate().GoToUrl(UrlSite[2]);
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("img")));
            var ImgAllObj = MyMozila.FindElements(By.CssSelector("img"));
            foreach (var ItemImg in ImgAllObj)
            {
                Console.WriteLine(ItemImg.GetAttribute("src"));
            }
        }

        static void Zadanie2()
        {
            Console.WriteLine("1.	Открыть форму «Нашли предложение лучше?» на https://msk.tele2.ru/, заполнить ее и закрыть (не отправлять!)");
            MyMozila.Navigate().GoToUrl(UrlSite[0]);
            var wait = new WebDriverWait(MyMozila, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("div[data-cartridge-type='EmptyElementWithClass']")));
            //сначало прокрутить страницу вниз
            var objGOTO = MyMozila.FindElement(By.CssSelector("div[data-cartridge-type='EmptyElementWithClass']"));
            IJavaScriptExecutor je = (IJavaScriptExecutor)MyMozila;
            je.ExecuteScript("arguments[0].scrollIntoView(true);", objGOTO);
            try
            {
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("iframe[id^='fl']")));
            }
            catch (WebDriverTimeoutException)
            {
                //не всегда подгружается фрейм
                Console.WriteLine("Форма не загрузилась, попробуйте еще раз. Задание прервано.");
                return;
            }
            var objFrom = MyMozila.FindElement(By.CssSelector("iframe[id^='fl']"));
             MyMozila.SwitchTo().Frame(objFrom);
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("button")));
            var objBtn = MyMozila.FindElement(By.CssSelector("button"));
            objBtn.Click();
            //наконец заполним форму
            MyMozila.FindElement(By.CssSelector("#tel")).SendKeys("9604422077");
            MyMozila.FindElement(By.CssSelector("#name")).SendKeys("Иван");
            //закроем ее
            //wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("section[class~='login']")));
            var objA = MyMozila.FindElement(By.CssSelector("section[class~='login'] a:first-child"));
            
            //objA.Click(); // ПОЧУМУ ТАК НЕ РАБОТАЕТ? ((((((((((((((
            ((IJavaScriptExecutor)MyMozila).ExecuteScript("arguments[0].click();", objA);
            Thread.Sleep(2000);//убедится что выше все отработало
            Console.WriteLine(objA.GetAttribute("Выполненно"));

            Console.WriteLine("2.	Открыть форму «Нашли предложение лучше?», открыть в новых окнах ссылки «правила"+
                "программы» и «обработка персональных данных», сделать скриншоты открытых страниц из кода и сохранить их"+
                "так, чтобы они лежали в том же месте, где и скрипт");
            //дабы не повторять выше код, просто выведу форму еще раз
            //objBtn.Click(); // уже не пашет
            ((IJavaScriptExecutor)MyMozila).ExecuteScript("arguments[0].click();", objBtn);
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("a[class=js-rules]")));
            //запонить ссылку на основное окно (вкладку)
            List<String> HomeHandel = MyMozila.WindowHandles.ToList();
            //открываем ссылы
            //MyMozila.FindElement(By.CssSelector("a[class=js-rules]")).Click();
            //MyMozila.FindElement(By.CssSelector("a[class=js-personal]")).Click();
            ((IJavaScriptExecutor)MyMozila).ExecuteScript("arguments[0].click();",
                MyMozila.FindElement(By.CssSelector("a[class=js-rules]")));
            ((IJavaScriptExecutor)MyMozila).ExecuteScript("arguments[0].click();",
                MyMozila.FindElement(By.CssSelector("a[class=js-personal]")));
            //делаем скриншоты
            Thread.Sleep(2000); // подождем пока создадутся окна.
            List<String> NewHandel = MyMozila.WindowHandles.ToList();
            Console.WriteLine("ОТкрыто всего окн - " + NewHandel.Count);
            NewHandel.Remove(HomeHandel[0]);// удалили Основную
            //Screenshot MyMozilaScrShot = ((ITakesScreenshot)MyMozila).GetScreenshot();
            ITakesScreenshot MyMozilaScrShotDriver = MyMozila as ITakesScreenshot;
            Screenshot MyMozilaScrShot;
            //String PutFile;
            foreach (var itemHandel in NewHandel)
            {
                MyMozila.SwitchTo().Window(itemHandel);
                Thread.Sleep(2000);
                //делаю скрин
                MyMozilaScrShot = MyMozilaScrShotDriver.GetScreenshot();
                MyMozilaScrShot.SaveAsFile(Environment.CurrentDirectory + "\\screen_" + DateTime.Now.Ticks + ".png");
                MyMozila.Close();
            }
            MyMozila.SwitchTo().Window(HomeHandel[0]);
        } 
        
        static void Zadanie3()
        {
            Console.WriteLine("1.	Сделать скрипт который бы прокликал поочередно по всем элементам выпадающего меню");
            MyMozila.Navigate().GoToUrl(UrlSite[0]);
            var wait = new WebDriverWait(MyMozila, TimeSpan.FromSeconds(15));
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".dropdown-menu")));
            int DropMenuCount = MyMozila.FindElements(By.CssSelector("ul[class='dropdown-menu'] > li")).Count;
            int DropMenuSubCount = 0;
            IWebElement DropMenu,
                        DropMenuSub;
            //var MyActions = new OpenQA.Selenium.Interactions.Actions(MyMozila);
            Console.WriteLine("Разделов меню: " + DropMenuCount);
            for (int i=0; i< DropMenuCount; i++)
            {
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("ul[class='dropdown-menu'] > li")));
                DropMenu = MyMozila.FindElements(By.CssSelector("ul[class='dropdown-menu'] > li"))[i];
                DropMenuSubCount = DropMenu.FindElements(By.CssSelector("div[class='submenu-col'] ul li a")).Count;
                Console.WriteLine("Ссылок "+ DropMenuSubCount  + " в "+(i+1)+" меню ");
                for(int j=0; j< DropMenuSubCount; j++)
                {
                    bool IsExit = false;
                    while (!IsExit)
                    {
                        try
                        {
                            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("ul[class='dropdown-menu'] > li")));
                            DropMenu = MyMozila.FindElements(By.CssSelector("ul[class='dropdown-menu'] > li"))[i];
                            DropMenu.Click(); // выбавет не показывает меню
                            //MyActions.MoveToElement(DropMenu).Build().Perform();
                            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("ul[class='dropdown-menu'] > li:nth-child(" + (i + 1) + ") div[class='submenu-col'] ul li a")));
                            DropMenuSub = DropMenu.FindElements(By.CssSelector("div[class='submenu-col'] ul li a"))[j];
                            Console.WriteLine(DropMenuSub.GetAttribute("href") + " " + (j + 1));
                            //break;
                            DropMenuSub.Click();
                            Thread.Sleep(1500); // увидеть результат
                            MyMozila.Navigate().GoToUrl(UrlSite[0]);
                            IsExit = true;
                        }
                        catch
                        {
                            Console.WriteLine("клик не прошел, пробуем еще раз");
                            MyMozila.Navigate().GoToUrl(UrlSite[0]);
                        }
                    }

                }
            }

            Console.WriteLine("2.	Сделать скрипт который бы прокликал поочередно по всем элементам выпадающего меню (один цикл)");
            MyMozila.Navigate().GoToUrl(UrlSite[0]);
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("div[class='submenu-col']")));
            Int32 objLinkForClickCount = MyMozila.FindElements(By.CssSelector("div[class='submenu-col'] ul li a")).Count;
            Console.WriteLine("Найдено " + objLinkForClickCount + " элементов");
            IWebElement itemLink;
            for (int i=0; i < objLinkForClickCount; i++)
            {
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("div[class='submenu-col'] ul li a")));
                itemLink = MyMozila.FindElements(By.CssSelector("div[class='submenu-col'] ul li a"))[i];
                Console.WriteLine(itemLink.GetAttribute("href") + " " + (i+1));
                ((IJavaScriptExecutor)MyMozila).ExecuteScript("arguments[0].click();", itemLink);
                wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("footer")));
                Thread.Sleep(1500); // увидеть результат
                //MyMozila.Navigate().Back(); // на странице https://msk.tele2.ru/shop/number?pageParams=type%3Dchoose%26price%3D0 не пашет
                MyMozila.Navigate().GoToUrl(UrlSite[0]); // тогда так
                //break;
            }
        }
        static void Zadanie4()
        {
            Console.WriteLine("1.	Путем установки переменных в скрипте (номер, пароль (не оставляй свои ) написать скрипт входа в личный кабинет");
            MyMozila.Navigate().GoToUrl(UrlSite[0]);
            var wait = new WebDriverWait(MyMozila, TimeSpan.FromSeconds(15));
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("div[class='actions-container']")));
            MyMozila.FindElement(By.CssSelector("div[class='actions-container'] div:nth-child(1) a")).Click();
            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("#loginDialog iframe")));
                //Console.WriteLine("iframe Формы авторизации загружен.");
            }
            catch (WebDriverTimeoutException)
            {
                //не всегда подгружается фрейм
                Console.WriteLine("Форма не загрузилась, попробуйте еще раз. Задание прервано.");
                return;
            }
            MyMozila.SwitchTo().Frame(MyMozila.FindElement(By.CssSelector("#loginDialog iframe")));
            while (true)
            {
                try
                {
                    wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("h1[class='h1']")));
                    break;
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("Форма не появилась, подождем");
                }
            }
            MyMozila.FindElement(By.CssSelector("li[data-tab='w-password']")).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("#phone-password")));
            MyMozila.FindElement(By.CssSelector("#phone-password")).SendKeys(Login);
            MyMozila.FindElement(By.CssSelector("#password-field")).SendKeys(Password);
            MyMozila.FindElement(By.CssSelector("#password-form input[value='Войти']")).Click();
            MyMozila.SwitchTo().DefaultContent();
            Console.WriteLine("Авторизация выполнена");

            Console.WriteLine("2.	Вывести баланс и остатки");
            wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("div[data-cartridge-type='LoginAction2']")));
            while(true)
                try
                {
                    MyMozila.FindElement(By.CssSelector("div[data-cartridge-type='LoginAction2']")).Click();
                    break;
                }
                catch (ElementClickInterceptedException)
                {
                Console.WriteLine("Клик не прошел.Пробую еще раз");
                Thread.Sleep(200);
                }
                catch
                {
                    Console.WriteLine("Кликнули");
                    break;
                }
            Thread.Sleep(1000);

            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("span[class='profile-popup_balance-value']")));
            Console.WriteLine("Баланс " + MyMozila.FindElement(By.CssSelector("span[class='profile-popup_balance-value']")).Text);
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div[class='profile-popup_rests profile-popup_block'] .profile-popup_rest")));
            Console.WriteLine("Остатки " + MyMozila.FindElement(By.CssSelector("div[class='profile-popup_rests profile-popup_block']")).Text);

            Console.WriteLine("3.	На странице «Расходы»: вывести в консоль все расходы с разбивкой по месяцам с глубиной -1 год от текущего");
            ((IJavaScriptExecutor)MyMozila).ExecuteScript("arguments[0].click();", MyMozila.FindElement(By.CssSelector("a[href='/lk/expenses']")));
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div[class='period-slider']")));
            for (int i = 1; i<=12; i++)
            { // требуется за 12 месяцев, но видимо интерфейс обновили и можно максимум по апрель
                wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div[class='expenses-category']")));
                Console.WriteLine("Месяц: " + MyMozila.FindElement(By.CssSelector("span[class='select-month'] span")).Text);
                var ObjElement = MyMozila.FindElements(By.CssSelector("div[class='expenses-category'] div[class='box slide-holder']"));
                foreach(var itemObjElement in ObjElement)
                {
                    var objDetal = itemObjElement.FindElements(By.CssSelector("div[class='hidden-xs'] li"));
                    Console.WriteLine(itemObjElement.FindElement(By.CssSelector("div[class='hidden-xs'] div[class='sum-line'] .title")).Text + " " +
                        itemObjElement.FindElement(By.CssSelector("div[class='hidden-xs'] div[class='sum-line'] .value")).Text);

                    foreach (var itemobjDetal in objDetal)
                    {
                        Console.WriteLine("> " + itemobjDetal.FindElement(By.CssSelector(".title")).Text + " " 
                            + itemobjDetal.FindElement(By.CssSelector(".info")).Text);
                    }

                }
                //прошли по расходам за месяц, теперь нажать на кнопку назад
                try
                {
                    MyMozila.FindElement(By.CssSelector("div[class='period-slider'] a[class^='prev']")).Click();
                }
                catch (NoSuchElementException)
                {
                    Console.WriteLine("Инфорфмации за " + DateTime.Now.AddMonths(-i).ToString("MMMM yyyy") + " недоступна. Завершение");
                    break;
                }
            }
        }

        static void Main(string[] args)
        {
            Int32 NomerZadaniya = 0;
            Console.WriteLine("Запускаем мозилу firefox на весь экран");
            MyMozila = new FirefoxDriver();
            MyMozila.Manage().Window.Maximize();
            do
            {
                Console.WriteLine("Введи номер задания 1-4. или 0 для выхода");
                NomerZadaniya = Convert.ToInt32(Console.ReadLine());
                switch (NomerZadaniya)
                {
                    case 1:
                        Zadanie1(); //первое задание
                        break;
                    case 2:
                        Zadanie2(); //второе задание
                        break;
                    case 3:
                        Zadanie3(); //третие задание
                        break;
                    case 4:
                        //Login = "****";
                        //Password = "****";
                        Console.WriteLine("Логин"); Login = Console.ReadLine();
                        Console.WriteLine("Пароль"); Password = Console.ReadLine();
                        Zadanie4(); //четвертое задание
                        break;
                    default:
                        if (NomerZadaniya != 0) Console.WriteLine("нет такого");
                        break;
                }
            } while (NomerZadaniya > 0);
            MyMozila.Close();
            MyMozila.Quit();
            return;
        }
    }
}
