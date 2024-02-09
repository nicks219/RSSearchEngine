import * as React from 'react';
import {useContext, useEffect, useState} from "react";
import {getNotesCount, getPageNumber, getCatalogPage} from "../common/dto.handlers";
import {Loader} from "../common/loader";
import {CatalogResponseDto} from "../dto/request.response.dto";
import {FunctionComponentStateWrapper} from "../common/state.wrappers";
import {CommonContext, RecoveryContext} from "../common/context.provider";

export const CatalogView = (): JSX.Element|undefined => {
    const [data, setData] = useState<CatalogResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper(mounted, setData);
    // чтоб не было ошибок на билде:
    const commonContext = useContext(CommonContext);
    const recoveryContext = useContext(RecoveryContext);

    useEffect(() => {
        Loader.unusedPromise = Loader.getDataById(stateWrapper, 1, Loader.catalogUrl);
        return function onUnmount() {
            mounted[0] = false;
            // перед выходом восстанавливаем состояние обёртки:
            commonContext.init();
        };
    }, []);

    const onClick = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let target = Number(e.currentTarget.id.slice(7));
        const item = {
            "pageNumber": getPageNumber(data),
            "direction": [target]
        };
        let requestBody = JSON.stringify(item);
        Loader.unusedPromise = Loader.postData(stateWrapper, requestBody, Loader.catalogUrl);
    }

    // стейт учитывается на этапе рендеринга
    // 0 - каталог; 1 - автоклик по ссылке для загрузки дампа; 2 - хз - загрузка первой страницы каталога и переход к "0"
    const onCreateDump = (e: React.SyntheticEvent) => {
        commonContext.createComponentState = 1;
        e.preventDefault();
        Loader.unusedPromise = Loader.getData(stateWrapper, Loader.migrationCreateUrl, recoveryContext);
    }

    const onRestoreDump = (e: React.SyntheticEvent) => {
        commonContext.createComponentState = 1;
        e.preventDefault();
        Loader.unusedPromise = Loader.getData(stateWrapper, Loader.migrationRestoreUrl, recoveryContext);
    }

    const onLogout = (e: React.SyntheticEvent) => {
        e.preventDefault();
        document.cookie = 'rsse_auth = false';
        let callback = (response: Response) => response.ok ? console.log("Logout Ok") : console.log("Logout Err");
        Loader.fireAndForgetWithQuery(Loader.logoutUrl, "", callback, stateWrapper, recoveryContext);
    }

    const onRedirect = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let noteId = Number(e.currentTarget.id);
        // по сути это переход на другой компонент, поэтому сбросим общий стейт:
        commonContext.init();
        Loader.redirectToMenu("/#/read/" + noteId);
    }

    const onDelete = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let id = Number(e.currentTarget.id);
        console.log('Try to delete song id: ' + id);
        Loader.unusedPromise = Loader.deleteDataById(stateWrapper, id, Loader.catalogUrl, getPageNumber(data), recoveryContext);
    }

    if (!data) return;

    const notes: JSX.Element[] = [];
    const notesArray = getCatalogPage(data);

    // работа с дампами:
    if (data.res && commonContext.createComponentState === 1) {
        notes.push(
            <tr key={"song "} className="bg-warning">
                <td></td>
                <td>{data.res}</td>
            </tr>);

        const link = document.createElement('a');
        link.href = data.res;
        link.download = data.res;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        commonContext.createComponentState = 2;
    }
    else if (data.res && commonContext.createComponentState === 2) {
        // после обработки дампа нажата кнопка "Каталог": а в данный момент это актуально?
        Loader.unusedPromise = Loader.getDataById(stateWrapper, 1, Loader.catalogUrl);
        commonContext.createComponentState = 0;
    }
    // на отладке можно получить пустой стейт и исключение:
    else if (notesArray) {
        for (let index = 0; index < notesArray.length; index++) {
            notes.push(
                <tr key={"song " + index} className="bg-warning">
                    <td></td>
                    <td>
                        <button className="btn btn-outline-light" id={notesArray[index].item2}
                                onClick={onRedirect}>{notesArray[index].item1}
                        </button>
                    </td>
                    <td>
                        <button className="btn btn-outline-light" id={notesArray[index].item2}
                                onClick={onDelete}>
                            &#10060;
                        </button>
                    </td>
                </tr>);
        }
    }

    return (
        <div className="row" id="renderContainer">
            <p style={{marginLeft: 12 + '%'}}>
                Всего песен: {getNotesCount(data)} &nbsp;
                Страница: {getPageNumber(data)} &nbsp;
            </p>
            <p></p>
            <p></p>
            <table className="table" id="catalogTable">
                <thead className="thead-dark ">
                <tr>
                    <th></th>
                    <th>
                        <form>
                            <button id="js-nav-1" className="btn btn-info" onClick={onClick}>
                                &lt;Назад
                            </button>
                            &nbsp;
                            <button id="js-nav-2" className="btn btn-info" onClick={onClick}>
                                Вперёд&gt;
                            </button>
                            &nbsp;
                            <button id="js-logout" className="btn btn-outline-light" onClick={onLogout}>
                                &lt;LogOut&gt;
                            </button>
                            <button id="js-logout" className="btn btn-outline-light" onClick={onCreateDump}>
                                &lt;Create&gt;
                            </button>
                            <button id="js-logout" className="btn btn-outline-light" onClick={onRestoreDump}>
                                &lt;Restore&gt;
                            </button>
                        </form>
                    </th>
                    <th></th>
                </tr>
                </thead>
                <tbody>
                {notes}
                </tbody>
            </table>
        </div>
    );
}
