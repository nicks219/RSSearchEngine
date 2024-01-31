import { CatalogDto } from "./catalog.dto.tsx";

// Catalog: страница каталога представляет из себя названия заметок и соответствующие им Id
export const getCatalogPage = (dto: CatalogDto) => dto.catalogPage;

// Catalog: количество заметок
export const getNotesCount = (dto: CatalogDto) => dto.notesCount;

// Catalog: номер страницы
export const getPageNumber = (dto: CatalogDto|null) => dto ? dto.pageNumber : 1;

