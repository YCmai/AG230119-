<template>
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header"></a-layout-header>
    <a-layout-content class="content-layout-content">
      <a-table
        :columns="columns"
        :data-source="data"
        bordered
        :rowKey="(row, index) => index"
        size="small"
        :pagination="pagination"
        @change="change"
        :loading="loading"
      >
      </a-table>
    </a-layout-content>
  </a-layout>
</template>
<script>
import component from "@/lib/base";
import locale from "@/localize/system/audit";
import { Get } from "@/api/audit";
export default {
  name: "AuditLog",
  mixins: [component],
  i18n: {
    messages: locale,
  },
  data() {
    return {
      columns: [
        {
          dataIndex: "userName",
          title: this.$t("title.userName"),
          align: "center",
        },
        {
          dataIndex: "url",
          title: this.$t("title.url"),
          align: "center",
          ellipsis: true,
        },
        {
          dataIndex: "executionTime",
          title: this.$t("title.time"),
          align: "center",
        },
        {
          dataIndex: "executionDuration",
          title: this.$t("title.duration"),
          align: "center",
        },
        {
          dataIndex: "httpStatusCode",
          title: this.$t("title.code"),
          align: "center",
          scopedSlots: { customRender: "code" },
        },
        {
          dataIndex: "exceptions",
          title: this.$t("title.exc"),
          align: "center",
          ellipsis: true,
        },
      ],
      data: [],
      loading: false,
      pagination: {
        current: 1,
        total: 0,
        pageSize: 10,
        "show-size-changer": true,
        "show-total": (total) => `共${total}条数据`,
      },
    };
  },
  mounted() {
    this.load();
  },
  methods: {
    load: async function () {
      let _this = this;
      _this.loading = true;
      try {
        let result = await Get({
          skipCount: (this.pagination.current - 1) * this.pagination.pageSize,
          maxResultCount: this.pagination.pageSize,
        });
        _this.pagination.total = result.totalCount;
        _this.data = result.items;
      } catch (err) {
        _this.$mesage.error(err.message);
      } finally {
        _this.loading = false;
      }
    },
    change: function (pagination) {
      this.pagination = pagination;
      this.load();
    },
  },
};
</script>
