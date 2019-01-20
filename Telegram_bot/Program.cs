using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using Telegram.Request;
using Telegram;
//Добавте токен!/Add the token!  В фаил/in file ->Settings.setting
//Добавте токен!/Add the token!  В фаил/in file ->Settings.setting
//Добавте токен!/Add the token!  В фаил/in file ->Settings.setting
namespace Telegram_bot
{
    class Program
    {
        static void Main(string[] args)
        {
            int ChatID = 243746390;
            TelegramRequest Tr = new TelegramRequest(Settings.Default.Token);
            Tr.MessageText += Tr_MessageText;
            Tr.MessageSticker += Tr_MessageSticker;
            Tr.MessagePhoto += Tr_MessagePhoto;
            Tr.MessageVideo += Tr_MessageVideo;
            Tr.MessageDocument += Tr_MessageDocument;
            Tr.MessageLocation += Tr_MessageLocation;
            Tr.MessageContact += Tr_MessageContact;
            Tr.MessageVoice += Tr_MessageVoice;
            Tr.GetUpdates();
            //Thread thr = new Thread(Tr.GetUpdates);
            //thr.IsBackground = true;
            //thr.Start();
            ////-----------------------МЕТОДЫ----------------------------------
            #region method
            //Method m = new Method(Settings.Default.Token);
            //m.Getme();
            //m.SendMessage("Ну привет!", 243746390);
            //m.ForwardMessage(243746390, 243746390, 467);

            //m.SendPhotoLink(243746390, "http://gallsource.com/wp-content/uploads/2014/08/Nice-Fantas-Tigers-Cat-Page-Wallpaper.jpg","Ky");
            //m.SendAudioIputFile(ChatID, @"C:\Users\XTreme.ws\Downloads\scream.mp3","Вам отправлен локальный фаил",9999999,"Тарзан","Поет");

            //m.SendDocumentIputFile(ChatID, @"C:\Users\XTreme.ws\Downloads\Evseeva.pdf");
            //m.SendDocumentoLink(ChatID, "http://www.t-krep.ru/files/ankera.pdf");

            //m.SendSticker(ChatID, "BQADAgADNgADyIsGAAFUgH2PcO7e6QI");

            // m.SendVideoInputFile(ChatID, @"C:\Users\XTreme.ws\Downloads\Керш 2 наркотические вещества RYTP.mp4");
            //m.SendVideoLink(ChatID, "http://cs05.userfiles.me/v/0/1474991977/2189314/x1/58d1c46728bd865d0daa61469a3ce3f3/Staraja_reklama_Twix._Xlektrika_vyzyvali-space.mp4");

            // m.SendVoiceInputFile(ChatID, @"C:\Users\XTreme.ws\Downloads\audio_2016-09-26_10-21-14.ogg");

            // m.SendLocation(ChatID, 51.482805f, -0.172431f);

            // m.SendVenue(ChatID, 40.717690f, -74.013885f, "Приходи к 5ти часам,не забудь!", "Chambers Street1");

            // m.SendContact(ChatID,"79196668880", "Dick", "Petrov");

            //m.SendChatAction(ChatID, Method.ChatAction.upload_document);

            //m.SendPhotoLink(ChatID, m.getUserProfilePhotos(ChatID, 0));

            //string[] aa = m.getUserProfilePhotosAllTime(ChatID, 0);

            //m.SendMessage(m.getFile("AgADAgADqacxG1ZGhw4jBAEaBjDnKjkocQ0ABFT1a221mxG7UdcBAAEC"), ChatID);
            #endregion
            Console.ReadLine();
            ////---------------------------------------------------------------
        }

        private static void Tr_MessageText(object sendr, MessageText e)
        {
            Console.WriteLine("ID сообщения:{0}\nID отправителя:{1}\nНик отправителя:{2}\nИмя:{3} Фамилия:{4}\nДата:{5}\nТекст сообщения:{6}",
               e.message_id, e.from.id, e.from.username, e.from.first_name, e.from.last_name, e.date, e.text );
        }
        private static void Tr_MessageSticker(object sendr, MessageSticker e)
        {
            Console.WriteLine("ID сообщения:{0}\nID отправителя:{1}\nНик отправителя:{2}\nИмя:{3} Фамилия:{4}\nДата:{5}\nШирина стикера:{6}  Высота стикера:{7}\nСмайлик:{8}\nЭскиз[id:{9} Размер:{10} Ширина:{11} Высота:{12}]\nID файла:{13}\nРазмер файла:{14}",
                e.message_id, e.from.id, e.from.username, e.from.first_name, e.from.last_name, e.date, e.width, e.height, e.emoji, e.thumb.file_id, e.thumb.file_size, e.thumb.width, e.thumb.height, e.file_id, e.file_size);
            Console.WriteLine();
        }
        private static void Tr_MessagePhoto(object sendr, MessagePhoto e)
        {
            Console.WriteLine("ID сообщения:{0}\nID отправителя:{1}\nНик отправителя:{2}\nИмя:{3} Фамилия:{4}\nДата:{5}\nКомментарий к фотографии:{6}",
                e.message_id, e.from.id, e.from.username, e.from.first_name, e.from.last_name, e.date, e.caption);
            for (int i = 0; i < e.photo.Count; i++)
            {
                Console.WriteLine("Фото №{0}",i+1);
                Console.WriteLine("ID Файла:{0}\nРазмер файла:{1}байт\nШирина:{2} Высота:{3}\n",
                    e.photo[i].file_id, e.photo[i].file_size, e.photo[i].width, e.photo[i].height);
            }
            Method m = new Method(Settings.Default.Token);
            m.SendMessage("Вы нам прислалим фотографию", 243746390);
            m.SendPhotoLink(243746390, e.photo[e.photo.Count - 1].file_id,e.caption);
        }
        private static void Tr_MessageVideo(object sendr, MessageVideo e)
        {
            Console.WriteLine("ID сообщения:{0}\nID отправителя:{1}\nНик отправителя:{2}\nИмя:{3} Фамилия:{4}\nДата:{5}\nID видео:{6}\nШирина:{7} Высота:{8}\nПродолжительность:{9}секунд\nРазмер:{10}байт\nЭскриз[ID видео:{11} Ширина:{12} Высота:{13} Размер:{14}]\nMime_type:{15}",
                 e.message_id, e.from.id, e.from.username, e.from.first_name, e.from.last_name, e.date, e.file_id, e.width, e.height, e.duration, e.file_size, e.thumb.file_id, e.thumb.width, e.thumb.height, e.thumb.file_size, e.mime_type);
            Console.WriteLine();
        }
        private static void Tr_MessageDocument(object sendr, MessageDocument e)
        {
            Console.WriteLine("ID сообщения:{0}\nID отправителя:{1}\nНик отправителя:{2}\nИмя:{3} Фамилия:{4}\nДата:{5}\nНазвание документа:{6}\nMime type:{7}\nID Документа:{8}\nРазмер документа:{9}байт",
                e.message_id, e.from.id, e.from.username, e.from.first_name, e.from.last_name, e.date, e.file_name, e.mime_type, e.file_id, e.file_size);
            Console.WriteLine("Эскиз[ID Документа:{0} Размер файла:{1}байт Ширина:{2} Высота:{3}]", e.thumb.file_id, e.thumb.file_size, e.thumb.width, e.thumb.height);
            Console.WriteLine();
        }
        private static void Tr_MessageLocation(object sendr, MessageLocation e)
        {
            Console.WriteLine("ID сообщения:{0}\nID отправителя:{1}\nНик отправителя:{2}\nИмя:{3} Фамилия:{4}\nДата:{5}\nШирина:{6} Долгота:{7}",
                e.message_id, e.from.id, e.from.username, e.from.first_name, e.from.last_name, e.date, e.latitude, e.longitude);
            Console.WriteLine();
        }
        private static void Tr_MessageContact(object sendr, MessageContact e)
        {
            Console.WriteLine("ID сообщения:{0}\nID отправителя:{1}\nНик отправителя:{2}\nИмя:{3} Фамилия:{4}\nДата:{5}\nНомер телефона:{6}\nИмя:{7} Фамилия:{8}\nID Пользователя(Если зарегистриован):{9}",
                 e.message_id, e.from.id, e.from.username, e.from.first_name, e.from.last_name, e.date, e.phone_number, e.first_name, e.last_name, e.user_id);
            Console.WriteLine();
        }
        private static void Tr_MessageVoice(object sendr, MessageVoice e)
        {
            Console.WriteLine("ID сообщения:{0}\nID отправителя:{1}\nНик отправителя:{2}\nИмя:{3} Фамилия:{4}\nДата:{5}\nID Аудиозаписи:{6}\nДлинна записи:{7}\nMime_type:{8}\nРазмер записи:{9}",
                  e.message_id, e.from.id, e.from.username, e.from.first_name, e.from.last_name, (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(e.date), e.file_id, e.duration, e.mime_type, e.file_size);
            Console.WriteLine();
        }

    }
}
