import * as React from 'react';
import {FC, JSX, ReactNode, useContext, useEffect, useState} from "react";
import {getPageNumber, getCatalogPage} from "../common/dto.handlers";
import {Loader} from "../common/loader";
import {CatalogResponseDto} from "../dto/request.response.dto";
import {FunctionComponentStateWrapper} from "../common/state.handlers";
import {CommonContext, RecoveryContext} from "../common/context.provider";
import {CatalogView} from "./catalog.view";
import {Dialog} from "../common/dialog.component.tsx";
import {Doms, Messages} from "../dto/doms.tsx";

export const CatalogContainer: FC = (): JSX.Element|undefined => {
    const actionTypeConfirmValue = "confirm";
    const [data, setData] = useState<CatalogResponseDto | null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper(mounted, setData);
    const recoveryContext = useContext(RecoveryContext);
    const commonContext = useContext(CommonContext);

    const [dialog, setDialog] = useState<ReactNode|null>(null);

    useEffect(() => {
        Loader.unusedPromise = Loader.getDataById(stateWrapper, 1, Loader.catalogPageGetUrl);
        return function onUnmount() {
            mounted[0] = false;
        };
    }, [data?.res]);

    const onClick = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let target = Number(e.currentTarget.id.slice(7));
        const item = {
            "pageNumber": getPageNumber(data),
            "direction": [target]
        };
        let requestBody = JSON.stringify(item);
        Loader.unusedPromise = Loader.postData(stateWrapper, requestBody, Loader.catalogNavigatePostUrl);
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
            Loader.unusedPromise = Loader.deleteDataById(stateWrapper, id, Loader.catalogDeleteNoteUrl, getPageNumber(data), recoveryContext);
        }

        askForConfirmation(onConfirm, Messages.confirmDelete);
    }

    async function downloadFile(fileName: string) {
        const response = await Loader.getFile(fileName);
        if (!response?.ok) {
            const errorText = await response?.text();
            console.error("error response:", errorText);
            throw new Error('dump download error');
        }

        const blob = await response.blob();
        const url = URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }

    if (!data) return;
    const elements: JSX.Element[] = [];
    if (data.res?.startsWith('dump') || data.res?.startsWith('backup')) {
        const dumpFileName = data.res;
        Loader.unusedPromise = downloadFile(dumpFileName);

        // вьюху с именем дампа пока спрячем
        // elements.push(
        //     <tr key={Doms.songWithSpace} className={Doms.bgWarning}>
        //         <td></td>
        //         <td>{dumpFileName}</td>
        //         <td></td>
        //     </tr>);
    }

    // данные для вьюхи: страничка каталога:
    const page = getCatalogPage(data);
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
        <div id={Doms.mainContent}>
            <CatalogView catalogDto={data} onClick={onClick} onLogout={onLogout}
                         onCreateDump={onCreateDump} onRestoreDump={onRestoreDump}
                         elements={elements}/>
            {dialog}
        </div>
    );
}
