// Catalog: страница каталога представляет из себя названия заметок и соответствующие им Id
export const getCatalogPage = (dto: any) => dto.catalogPage;

// Catalog: количество заметок
export const getNotesCount = (dto: any) => dto.notesCount;

// Catalog: номер страницы
export const getPageNumber = (dto: any) => dto ? dto.pageNumber : 1;
