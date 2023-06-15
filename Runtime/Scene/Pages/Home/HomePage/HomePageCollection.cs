using System.Collections.Generic;
using BeWild.AIBook.Runtime.Data;
using BeWild.AIBook.Runtime.Scene.Pages.Home.BookList;

namespace BeWild.AIBook.Runtime.Scene.Pages.Home.HomePage
{
    public class HomePageCollection : BookListUI
    {
        public void SetData(List<CollectionData> data)
        {
            ClearBooks();

            List<BookBriefData> books = new List<BookBriefData>();
            foreach (CollectionData collectionData in data)
            {
                books.Add(new BookBriefData()
                {
                    id = collectionData.Id,
                    name = collectionData.Name,
                    icon = collectionData.ImageUrl
                });
            }

            AddBooks(new BookListData()
            {
                books = books,
            });
        }
    }
}