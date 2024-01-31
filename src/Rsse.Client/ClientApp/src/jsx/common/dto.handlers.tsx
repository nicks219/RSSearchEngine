import { CatalogResponseDto, NoteResponseDto, NoteRequestDto } from "../dto/request.response.dto.tsx";
import { ISimpleProps } from "./contracts.tsx";


// Catalog: страница каталога представляет из себя названия заметок и соответствующие им Id
export const getCatalogPage = (dto: CatalogResponseDto) => dto.catalogPage;

// Catalog: количество заметок
export const getNotesCount = (dto: CatalogResponseDto) => dto.notesCount;

// Catalog: номер страницы
export const getPageNumber = (dto: CatalogResponseDto|null) => dto ? dto.pageNumber : 1;


// Note: именование заметки в запросе
export const getTitleRequest = (dto: NoteRequestDto) => dto.titleRequest;

// Note: текст заметки в запросе
export const getTextRequest = (dto: NoteRequestDto) => dto.textRequest;

// Note: func(tagsCheckedUncheckedResponse) представление списка тегов в виде строк "отмечено-не отмечено" в ответе
export const getTagsCheckedUncheckedResponse = (props: Readonly<ISimpleProps>) : string|undefined => {
    if (props.jsonStorage?.tagsCheckedUncheckedResponse) return props.jsonStorage.tagsCheckedUncheckedResponse[Number(props.id)];
};

// Note: получить именование заметки в ответе
export const getTitleResponse = (jsonStorage?: NoteResponseDto) => jsonStorage?.titleResponse;

// Note: выставить именование заметки в ответе
export const setTitleResponse = (jsonStorage?: NoteResponseDto, value?: string) => { if (jsonStorage) jsonStorage.titleResponse = value };

// Note: получить текст заметки в ответе
export const getTextResponse = (dto?: NoteResponseDto) => dto?.textResponse;

// Note: выставить текст заметки в ответе
export const setTextResponse = (dto?: NoteResponseDto, value?: string) => { if (dto) dto.textResponse = value };

// Note: список тегов в формате "имя : количество записей"
export const getStructuredTagsListResponse = (dto?: NoteResponseDto) => dto?.structuredTagsListResponse ?? [];

// Note: поле для хранения идентификатора сохраненной/измененной заметки
export const getCommonNoteId = (dto: NoteResponseDto) => dto.commonNoteID;
