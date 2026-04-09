import axios from '@/lib/axios-extension';
import url from '@/lib/url-normalize';
export function Get(input) {
    return axios.get(`/api/audit/get?${url.normalize(input)}`);
}