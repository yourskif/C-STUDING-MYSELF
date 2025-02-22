using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва книги є обов’язковою")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Кількість сторінок є обов’язковою")]
        [Range(1, 5000, ErrorMessage = "Кількість сторінок має бути від 1 до 5000")]
        public int PageCount { get; set; }

        [Required(ErrorMessage = "Жанр є обов’язковим")]
        public GenreEnum Genre { get; set; }
<<<<<<< HEAD

        public int AuthorId { get; set; }

        public Author Author { get; set; }
    }
}
=======
    }
}
>>>>>>> d2817a6381a9ac03b7009984a0f352cb75ecedb8
