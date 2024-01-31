﻿import * as React from 'react';
import { useEffect, useState } from "react";
import { getNotesCount, getPageNumber, getCatalogPage } from "../common/dto.handlers.tsx";
import { Loader } from "../common/loader.tsx";
import { CatalogResponseDto } from "../dto/request.response.dto.tsx";
import { DumpStateWrapper, FunctionComponentStateWrapper } from "../common/state.wrappers.tsx";

export const CatalogView = (): JSX.Element|undefined => {
    const [data, setData] = useState<CatalogResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper(mounted, setData, data);

    useEffect(() => {
        Loader.unusedPromise = Loader.getDataById<CatalogResponseDto>(stateWrapper, 1, Loader.catalogUrl);
        return function onUnmount() {
            mounted[0] = false
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

    const onCreateDump = (e: React.SyntheticEvent) => {
        DumpStateWrapper.setState(1);
        e.preventDefault();
        Loader.unusedPromise = Loader.getData(stateWrapper, Loader.migrationCreateUrl);
    }

    const onRestoreDump = (e: React.SyntheticEvent) => {
        DumpStateWrapper.setState(1);
        e.preventDefault();
        Loader.unusedPromise = Loader.getData(stateWrapper, Loader.migrationRestoreUrl);
    }

    const onLogout = (e: React.SyntheticEvent) => {
        e.preventDefault();
        document.cookie = 'rsse_auth = false';
        let callback = (response: Response) => response.ok ? console.log("Logout Ok") : console.log("Logout Err");
        Loader.fireAndForgetWithQuery(Loader.logoutUrl, "", callback, stateWrapper);
    }

    const onRedirect = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let noteId = Number(e.currentTarget.id);
        Loader.redirectToMenu("/#/read/" + noteId);
    }

    const onDelete = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let id = Number(e.currentTarget.id);
        console.log('You want to delete song with id: ' + id);
        Loader.unusedPromise = Loader.deleteDataById(stateWrapper, id, Loader.catalogUrl, getPageNumber(data));
    }

    if (!data) return;

    let notes: JSX.Element[] = [];
    let itemArray = getCatalogPage(data);

    // работа с дампами:
    if (data.res && DumpStateWrapper.getState() === 1) {
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

        DumpStateWrapper.setState(2);
    } else if (data.res && DumpStateWrapper.getState() === 2) {
        // после обработки дампа нажата кнопка "Каталог":
        Loader.unusedPromise = Loader.getDataById<CatalogResponseDto>(stateWrapper, 1, Loader.catalogUrl);// вместо DidMount
        DumpStateWrapper.setState(0);
    }
    // на отладке можно получить пустой стейт и исключение:
    else if (itemArray !== null && itemArray !== undefined) {
        for (let i = 0; i < itemArray.length; i++) {
            notes.push(
                <tr key={"song " + i} className="bg-warning">
                    <td></td>
                    <td>
                        <button className="btn btn-outline-light" id={itemArray[i].item2}
                                onClick={onRedirect}>{itemArray[i].item1}
                        </button>
                    </td>
                    <td>
                        <button className="btn btn-outline-light" id={itemArray[i].item2}
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
