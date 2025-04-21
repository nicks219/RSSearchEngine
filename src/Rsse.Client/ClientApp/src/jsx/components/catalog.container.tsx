import * as React from 'react';
import {ReactNode, useContext, useEffect, useState} from "react";
import {getPageNumber, getCatalogPage} from "../common/dto.handlers";
import {Loader} from "../common/loader";
import {CatalogResponseDto} from "../dto/request.response.dto";
import {FunctionComponentStateWrapper} from "../common/state.handlers";
import {CommonContext, RecoveryContext} from "../common/context.provider";
import {CatalogView} from "./catalog.view";
import {Dialog} from "../common/dialog.component.tsx";
import {Doms, Messages} from "../dto/doms.tsx";

export const CatalogContainer = (): JSX.Element|undefined => {
    const actionTypeConfirmValue = "confirm";
    const [data, setData] = useState<CatalogResponseDto | null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper(mounted, setData);
    const recoveryContext = useContext(RecoveryContext);
    const commonContext = useContext(CommonContext);

    const [dialog, setDialog] = useState<ReactNode|null>(null);

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

    const askForConfirmation = (onConfirm: () => void, header: string) => {
        const message: string = "Отменить это действие будет невозможно";
        setDialog(
            <Dialog onAction={(action: string) => {
                setDialog(null);
                if (action === actionTypeConfirmValue) {
                    console.log(action);
                    onConfirm();
                }
            }} header={header}>
                {message}
            </Dialog>
        );
    }

    const onRestoreDump = (e: React.SyntheticEvent) => {
        e.preventDefault();
        const onConfirm = () => {
            Loader.unusedPromise = Loader.getData(stateWrapper, Loader.migrationRestoreUrl, recoveryContext);
        }
        askForConfirmation(onConfirm, Messages.confirmDumpRestore);
    }

    const onLogout = (e: React.SyntheticEvent) => {
        e.preventDefault();

        document.cookie = 'rsse_auth = false';
        localStorage.setItem('isAuth', 'false');
        commonContext.stringState?.(Doms.submitStyle);

        let callback = (response: Response) => response.ok ? console.log(Messages.logoutOk) : console.log(Messages.logoutErr);
        Loader.fireAndForgetWithQuery(Loader.logoutUrl, "", callback, stateWrapper, recoveryContext);
    }

    const onRedirect = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let noteId = Number(e.currentTarget.id);
        Loader.redirectToMenu("/#/read/" + noteId);
    }

    const onDelete = (e: React.SyntheticEvent) => {
        e.preventDefault();
        const targetId = e.currentTarget.id;
        const onConfirm = () => {
            let id = Number(targetId);
            console.log('Try to delete song id: ' + id);
            Loader.unusedPromise = Loader.deleteDataById(stateWrapper, id, Loader.catalogUrl, getPageNumber(data), recoveryContext);
        }

        askForConfirmation(onConfirm, Messages.confirmDelete);
    }

    if (!data) return;
    const elements: JSX.Element[] = [];
    const page = getCatalogPage(data);

    // данные для вьюхи: имя файла дампа:
    if (data.res) {
        elements.push(
            <tr key={Doms.songWithSpace} className={Doms.bgWarning}>
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
                <tr key={Doms.songWithSpace + index} className={Doms.bgWarning}>
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

    return (
        <div>
            <CatalogView catalogDto={data} onClick={onClick} onLogout={onLogout}
                         onCreateDump={onCreateDump} onRestoreDump={onRestoreDump}
                         elements={elements}/>
            {dialog}
        </div>
    );
}
