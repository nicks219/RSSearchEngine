import {Loader} from "../common/loader";
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';

// не является реальным тестом, скорее знакомство с фреймворком тестирования
describe('Loader with mocked static method', () => {
    beforeEach(() => {
        // Сохраняем оригинальный метод, чтобы потом восстановить
        vi.spyOn(Loader, 'getData').mockImplementation(async (
            undefined, url, recoveryContext) => {
            if (url.includes('success')) {
                return { id: 1, name: 'Mocked Data' };
            } else {
                throw new Error('Mocked Network Error');
            }
        });
    });

    afterEach(() => {
        vi.restoreAllMocks();
    });

    it('should return mocked data', async () => {
        const result = await Loader.getData(undefined,'success', undefined);
        expect(result).toEqual({ id: 1, name: 'Mocked Data' });
    });

    it('should throw mocked error', async () => {
        // const arg = useState(false);
        // const stateWrapper = new FunctionComponentStateWrapper(arg, null);
        const stateWrapper = undefined;

        await expect(Loader.getData(undefined, 'error')).rejects.toThrow('Mocked Network Error');
    });
});
