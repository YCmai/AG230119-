import axios from '@/lib/axios-extension';
import url from '@/lib/url-normalize';
export function GetList(input) {
    return axios.get("/api/wms/barcode/getlist?" + url.normalize(input));
}