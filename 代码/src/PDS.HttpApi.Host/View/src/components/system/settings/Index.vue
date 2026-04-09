<template>
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header"></a-layout-header>
    <a-layout-content class="content-layout-content">
      <a-table
        :columns="columns"
        :data-source="data"
        bordered
        :rowKey="(row, index) => row.name"
        size="small"
        :pagination="pagination"
        @change="change"
        :loading="loading"
      >
        <template slot="isDeleted" slot-scope="text, record">
          {{ record.isDeleted ? $t("title.forbidden") : $t("title.enable") }}
        </template>
        <template slot="operation" slot-scope="text, record">
          <a
            href="javascript:void(0);"
            @click="show('edit', record)"
            v-if="isGranted('Setting.Global.Update')"
          >
            <a-icon type="edit" />{{ $t("actions.edit") }}
          </a>
        </template>
      </a-table>
    </a-layout-content>
    <Form ref="form" v-if="showForm" @close="close" />
  </a-layout>
</template>
<script>
import Form from "./Form.vue";
import component from "@/lib/base";
import { Pagination } from "@/api/setting";
import locale from "@/localize/system/settings";
export default {
  name: "SettingManager",
  components: {
    Form,
  },
  mixins: [component],
  i18n: {
    messages: locale,
  },
  data() {
    return {
      showForm: false,
      loading: false,
      columns: [
        {
          dataIndex: "displayName",
          title: this.$t("title.displayName"),
        },
        {
          dataIndex: "currentValue",
          title: this.$t("title.value"),
        },
        {
          dataIndex: "description",
          title: this.$t("title.description"),
        },
        {
          dataIndex: "Actions",
          title: this.$t("title.actions"),
          align: "center",
          scopedSlots: { customRender: "operation" },
          width: 150,
        },
      ],
      data: [],
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
      try {
        _this.loading = true;
        let response = await Pagination({
          skipCount: (this.pagination.current - 1) * this.pagination.pageSize,
          maxResultCount: this.pagination.pageSize,
        });
        _this.data = response.items;
        _this.pagination.total = response.totalCount;
      } catch (err) {
      } finally {
        _this.loading = false;
      }
    },
    show: function (action, data) {
      let _this = this;
      _this.showForm = true;
      _this.$nextTick(() => {
        _this.$refs.form.load(data);
      });
    },
    close: function () {
      this.showForm = false;
      this.load();
    },
    change: function (pagination) {
      this.pagination = pagination;
      this.load();
    },
  },
};
</script>
