import * as React from "react";
import {Dispatch, SetStateAction} from "react";

/** Обёртка, позволяющая загрузчику менять стейт функциональных компонентов приложения */
export class FunctionComponentStateWrapper<T> {
    public mounted: [boolean, React.Dispatch<React.SetStateAction<boolean>>];
    public setData: Dispatch<SetStateAction<T|null>>;

    constructor(mounted: [boolean, React.Dispatch<React.SetStateAction<boolean>>],
                setData: Dispatch<SetStateAction<T|null>>) {
        this.mounted = mounted;
        this.setData = setData;
    }
}

/** Глобальный стейт с начальной инициализацией части полей */
export class CommonStateStorage {
    // .. с начальной инициализацией ..

    /** CreateComponent.SubmitButton state: режим "подтверждение/отмена" */
    public static jsonStringStorage: string = "";// сохранение/восстановление заметки при отмене и подтверждение в режиме "подтверждение/отмена":

    // .. начальная инициализация не обязательна (?), перенесено из интерфейса Window ..
    // сделать numberStorage - stringStorage - boolStorage ?

    /** Передача номера заметки между компонентами */
    public static noteIdStorage: number = 0;
    /** Выставляется в SetVisible<T> */
    public static stateWrapperStorage: FunctionComponentStateWrapper<any>;
    /** Выставляется в SetVisible<T> */
    public static urlStorage: string;


    /** I: CatalogComponent: работа с дампами; II: CreateComponent: режима "подтверждение/отмена" */
    private static _commonState: number = 0;
    public static get commonState() {return CommonStateStorage._commonState};
    public static set commonState(state: number) {CommonStateStorage._commonState = state;}


    /** Восстановление начальных значений */
    static init = () => {
        // они точно не взаимозаменяемы?
        this._commonState = 0;
        // this.redirectState = 0;
        this.jsonStringStorage = "";
    }

    /** ReadComponent: редирект из каталога */
    //private static _redirectState: number = 0;
    //static get redirectState() {return this._redirectState};
    //static set redirectState(i: number) {this._redirectState = i};
}

// catalog: (1) setState: работа с дампами.
// create: (2) режим поиска похожих заметок.
// read: (2) вызов из каталога.
// window: используется в "общем" функционале.
