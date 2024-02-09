import * as React from 'react';
import {useContext, useEffect, useState} from "react";
import {getNotesCount, getPageNumber, getCatalogPage} from "../common/dto.handlers";
import {Loader} from "../common/loader";
import {CatalogResponseDto} from "../dto/request.response.dto";
import {FunctionComponentStateWrapper} from "../common/state.wrappers";
import {RecoveryContext} from "../common/context.provider";

export const CatalogContainer = (): JSX.Element|undefined => {
    const [data, setData] = useState<CatalogResponseDto | null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper(mounted, setData);
    const recoveryContext = useContext(RecoveryContext);

    useEffect(() => {
        Loader.unusedPromise = Loader.getDataById(stateWrapper, 1, Loader.catalogUrl);
        return function onUnmount() {
            mounted[0] = false;
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
        e.preventDefault();
        Loader.unusedPromise = Loader.getData(stateWrapper, Loader.migrationCreateUrl, recoveryContext);
    }

    const onRestoreDump = (e: React.SyntheticEvent) => {
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
        Loader.redirectToMenu("/#/read/" + noteId);
    }

    const onDelete = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let id = Number(e.currentTarget.id);
        console.log('Try to delete song id: ' + id);
        Loader.unusedPromise = Loader.deleteDataById(stateWrapper, id, Loader.catalogUrl, getPageNumber(data), recoveryContext);
    }

    if (!data) return;
    const elements: JSX.Element[] = [];
    const page = getCatalogPage(data);

    // данные для вьюхи: имя файла дампа:
    if (data.res) {
        elements.push(
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
    }

    // данные для вьюхи: страничка каталога:
    if (page) {
        for (let index = 0; index < page.length; index++) {
            elements.push(
                <tr key={"song " + index} className="bg-warning">
                    <td></td>
                    <td>
                        <button className="btn btn-outline-light"
                                // note id and name:
                                id={page[index].item2}
                                onClick={onRedirect}>{page[index].item1}
                        </button>
                    </td>
                    <td>
                        <button className="btn btn-outline-light"
                                // note id:
                                id={page[index].item2}
                                onClick={onDelete}>
                            &#10060;
                        </button>
                    </td>
                </tr>);
        }
    }

    return (<CatalogView catalogDto={data} onClick={onClick} onLogout={onLogout}
                         onCreateDump={onCreateDump} onRestoreDump={onRestoreDump}
                         elements={elements}/>);
}

export const CatalogView = (props: {catalogDto:CatalogResponseDto,
                     onClick:(e:React.SyntheticEvent)=>void,
                     onLogout:(e:React.SyntheticEvent)=>void,
                     onCreateDump:(e:React.SyntheticEvent)=>void,
                     onRestoreDump:(e:React.SyntheticEvent)=>void,
                     elements:JSX.Element[]}) => {
    return (
        <div className="row" id="renderContainer">
            <p style={{marginLeft: 12 + '%'}}>
                Всего песен: {getNotesCount(props.catalogDto)} &nbsp;
                Страница: {getPageNumber(props.catalogDto)} &nbsp;
            </p>
            <p></p>
            <p></p>
            <table className="table" id="catalogTable">
                <thead className="thead-dark ">
                <tr>
                    <th></th>
                    <th>
                        <form>
                            <button id="js-nav-1" className="btn btn-info" onClick={props.onClick}>
                                &lt;Назад
                            </button>
                            &nbsp;
                            <button id="js-nav-2" className="btn btn-info" onClick={props.onClick}>
                                Вперёд&gt;
                            </button>
                            &nbsp;
                            <button id="js-logout" className="btn btn-outline-light" onClick={props.onLogout}>
                                &lt;LogOut&gt;
                            </button>
                            <button id="js-logout" className="btn btn-outline-light" onClick={props.onCreateDump}>
                                &lt;Create&gt;
                            </button>
                            <button id="js-logout" className="btn btn-outline-light" onClick={props.onRestoreDump}>
                                &lt;Restore&gt;
                            </button>
                        </form>
                    </th>
                    <th></th>
                </tr>
                </thead>
                <tbody>{props.elements}</tbody>
            </table>
        </div>
    );
}
